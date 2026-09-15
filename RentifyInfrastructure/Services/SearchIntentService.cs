using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using RentifyApplication.Exceptions;
using RentifyApplication.IServices;
using RentifyApplication.Query.SearchRentals;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyDomain.Enum;
using RentifyInfrastructure.Mappers;
using RentifyInfrastructure.Metrics;
using RentifyInfrastructure.Models;
using System.ClientModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RentifyInfrastructure.Services;

public sealed class SearchIntentService : ISearchIntentService
{
    private readonly ResponsesClient _client;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly ILogger<SearchIntentService> _logger;

    private readonly LlmMetrics _llmMetrics;

    public SearchIntentService(ResponsesClient client, IConfiguration configuration, ILogger<SearchIntentService> logger, LlmMetrics llmMetrics)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _llmMetrics = llmMetrics;
    }

    public async Task<SearchIntent> CreateIntentAsync(SearchRentalsQuery query, CancellationToken cancellationToken)
    {
        var model = _configuration["OpenAI:Model"]
            ?? throw new InvalidOperationException(
                "OpenAI model is not configured.");

        var options = new CreateResponseOptions
        {
            Model = model,
            MaxOutputTokenCount = 256,
            TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "search_intent", jsonSchema: SearchIntentSchema.Create())
            }
        };

        options.TextOptions.Patch.Set("$.verbosity"u8, "low");

        options.InputItems.Add(
            ResponseItem.CreateSystemMessageItem(
            """
            Extract rental search criteria from the user's query.

            Rules:
            - Extract only stated or clearly implied information.
            - Normalize obvious synonyms.
            - cityCode must be the Turkish province plate code.
            - Convert natural-language dates to yyyy-MM-dd when they can be determined.
            - Currency must be a 3-letter ISO currency code such as TRY, USD or EUR.
            """));

        options.InputItems.Add(ResponseItem.CreateUserMessageItem(query.Query));

        ClientResult<ResponseResult> response;
        var stopwatch = Stopwatch.StartNew();

        try
        {
            response = await _client.CreateResponseAsync(options, cancellationToken);
        }
        catch (ClientResultException ex)
        {
            _llmMetrics.RecordFailure(model);

            var (code, statusCode, message) = ex.Status switch
            {
                401 => (
                    "LLM_AUTHENTICATION_ERROR",
                    502,
                    "Search service authentication failed."),

                429 => (
                    "LLM_RATE_LIMITED",
                    503,
                    "Search service is temporarily busy."),

                >= 500 => (
                    "LLM_SERVICE_ERROR",
                    503,
                    "Search service is temporarily unavailable."),

                _ => (
                    "LLM_REQUEST_ERROR",
                    502,
                    "Search service could not process the request.")
            };

            throw new LlmServiceException(code, statusCode, message, ex);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogDebug("LLM request completed. Model: {Model}, DurationMs: {DurationMs}", model, stopwatch.ElapsedMilliseconds);
        }

        var output = response.Value.GetOutputText();
        var usage = response.Value.Usage;

        _llmMetrics.RecordRequest(model, usage.InputTokenCount, usage.OutputTokenCount, stopwatch.Elapsed.TotalMilliseconds);

        _logger.LogInformation(
            "LLM usage. Model: {Model}, InputTokens: {InputTokens}, OutputTokens: {OutputTokens}, TotalTokens: {TotalTokens}, DurationMs: {DurationMs}",
            model,
            usage.InputTokenCount,
            usage.OutputTokenCount,
            usage.TotalTokenCount,
            stopwatch.ElapsedMilliseconds);

        SearchIntentModel? modelResult;

        try
        {
            modelResult = JsonSerializer.Deserialize<SearchIntentModel>(output, JsonOptions);
        }
        catch (JsonException ex)
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid response.", ex);
        }

        if (modelResult is null)
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an empty response.");
        }

        ValidateModelResult(modelResult);

        return SearchIntentMapper.Map(modelResult, query.Page);
    }

    private static void ValidateModelResult(SearchIntentModel model)
    {
        if (!Enum.TryParse<RentalType>(model.RentalType, ignoreCase: true, out var rentalType) || !Enum.IsDefined(rentalType))
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid rental type.");
        }

        if (model.CityCode != 0 && !Enum.IsDefined(typeof(CityCode), model.CityCode))
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid city code.");
        }

        DateOnly? startDate = null;
        DateOnly? endDate = null;

        if (!string.IsNullOrWhiteSpace(model.StartDate))
        {
            if (!DateOnly.TryParseExact(model.StartDate, "yyyy-MM-dd", out var parsedStartDate))
            {
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid start date.");
            }

            startDate = parsedStartDate;
        }

        if (!string.IsNullOrWhiteSpace(model.EndDate))
        {
            if (!DateOnly.TryParseExact(model.EndDate, "yyyy-MM-dd", out var parsedEndDate))
            {
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid end date.");
            }

            endDate = parsedEndDate;
        }

        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid date range.");
        }

        if (!string.IsNullOrWhiteSpace(model.Currency))
        {
            if (!Enum.TryParse<Currency>(model.Currency, ignoreCase: true, out var currency) || !Enum.IsDefined(currency))
            {
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid currency.");
            }
        }

        if (model.MinPrice.HasValue && model.MinPrice.Value < 0 || model.MaxPrice.HasValue && model.MaxPrice.Value < 0)
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid price.");
        }

        if (model.MinPrice.HasValue && model.MaxPrice.HasValue && model.MinPrice.Value > model.MaxPrice.Value)
        {
            throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid price range.");
        }

        switch (rentalType)
        {
            case RentalType.Vehicle when model.VehicleCriteria is null:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned missing vehicle criteria.");

            case RentalType.Vehicle when model.VehicleCriteria.Seats is <= 0:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid seat count.");

            case RentalType.Property or RentalType.Villa when model.PropertyCriteria is null:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned missing property criteria.");

            case RentalType.Hotel when model.HotelCriteria is null:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned missing hotel criteria.");

            case RentalType.Hotel when model.HotelCriteria.Stars is < 1 or > 5:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid hotel star rating.");

            case RentalType.Hotel when model.HotelCriteria.GuestCapacity is <= 0:
                throw new LlmServiceException("LLM_INVALID_RESPONSE", 502, "Search service returned an invalid guest capacity.");
        }
    }
}