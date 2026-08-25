using Microsoft.Extensions.Configuration;
using OpenAI.Responses;
using RentifyApplication.IServices;
using RentifyApplication.Query.SearchRentals.SearchCriteria;
using RentifyInfrastructure.Mappers;
using RentifyInfrastructure.Models;
using System.Text.Json;

namespace RentifyInfrastructure.Services;

#pragma warning disable OPENAI001
public sealed class SearchIntentService : ISearchIntentService
{
    private readonly ResponsesClient _client;
    private readonly IConfiguration _configuration;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public SearchIntentService(ResponsesClient client, IConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public async Task<SearchIntent> CreateIntentAsync(string query, CancellationToken cancellationToken)
    {
        var model = _configuration["OpenAI:Model"]
            ?? throw new InvalidOperationException(
                "OpenAI model is not configured.");

        var options = new CreateResponseOptions
        {
            Model = model,
            TextOptions = new ResponseTextOptions
            {
                TextFormat = ResponseTextFormat.CreateJsonSchemaFormat(jsonSchemaFormatName: "search_intent", jsonSchema: SearchIntentSchema.Create())
            }
        };

        options.InputItems.Add(
            ResponseItem.CreateSystemMessageItem(
                """
            You are Rentify's search intent extraction engine.
            Convert the user's natural-language rental request into structured search criteria.

            Supported rental types:
            Vehicle, Property, Hotel, Unknown.

            Rules:
            - Never invent information.
            - Extract only stated or clearly implied information.
            - Use null for unspecified values.
            - Use Unknown when the rental type is unclear.
            - Normalize obvious synonyms.
            - Extract location, dates and price only when provided.
            - Extract type-specific criteria when provided.
            - Do not perform the search.
            """));

        options.InputItems.Add(ResponseItem.CreateUserMessageItem(query));

        var response = await _client.CreateResponseAsync(options, cancellationToken);

        var output = response.Value.GetOutputText();

        var modelResult = JsonSerializer.Deserialize<SearchIntentModel>(output, JsonOptions);

        if (modelResult is null)
        {
            throw new InvalidOperationException("The search intent could not be parsed.");
        }

        return SearchIntentMapper.Map(modelResult);
    }
}

#pragma warning restore OPENAI001