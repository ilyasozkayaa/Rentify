using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using RentifyApplication.Constants;
using RentifyApplication.IServices;

namespace RentifyApi.Idempotency;

public sealed class IdempotencyFilter : IAsyncActionFilter
{
    private readonly IIdempotencyStore _idempotencyStore;
    private readonly IConfiguration _configuration;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public IdempotencyFilter(IIdempotencyStore idempotencyStore, IConfiguration configuration, IOptions<Microsoft.AspNetCore.Mvc.JsonOptions> jsonOptions)
    {
        _idempotencyStore = idempotencyStore;
        _configuration = configuration;
        _jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ProjectConstants.HeaderName, out var headerValue) || string.IsNullOrWhiteSpace(headerValue))
        {
            context.Result = new BadRequestObjectResult(new { message = $"{ProjectConstants.HeaderName} header is required." });
            return;
        }

        var idempotencyKey = headerValue.ToString();

        if (idempotencyKey.Length > 128)
        {
            context.Result = new BadRequestObjectResult(new { message = $"{ProjectConstants.HeaderName} cannot exceed 128 characters." });
            return;
        }

        var userId = context.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (string.IsNullOrWhiteSpace(userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var endpoint = context.HttpContext.Request.Path.Value ?? "unknown";
        var storeKey = $"idempotency:{userId}:{endpoint}:{idempotencyKey}";

        var ttlSeconds = Math.Max(1, _configuration.GetValue("Idempotency:TtlSeconds", 3600));
        var ttl = TimeSpan.FromSeconds(ttlSeconds);

        var result = await _idempotencyStore.TryStartAsync(storeKey, ttl, context.HttpContext.RequestAborted);

        if (result.IsProcessing)
        {
            context.Result = new ConflictObjectResult(new { message = "This request is already being processed. Please try again." });
            return;
        }

        if (!result.Acquired)
        {
            context.Result = new ContentResult
            {
                Content = result.Response,
                ContentType = result.ContentType ?? "application/json",
                StatusCode = result.StatusCode ?? StatusCodes.Status200OK
            };

            return;
        }

        ActionExecutedContext executedContext;

        try
        {
            executedContext = await next();
        }
        catch
        {
            await _idempotencyStore.RemoveAsync(storeKey, CancellationToken.None);
            throw;
        }

        if (executedContext.Exception is not null && !executedContext.ExceptionHandled)
        {
            await _idempotencyStore.RemoveAsync(storeKey, CancellationToken.None);
            return;
        }

        if (executedContext.Result is not ObjectResult objectResult)
        {
            await _idempotencyStore.RemoveAsync(storeKey, CancellationToken.None);
            return;
        }

        var responseBody = JsonSerializer.Serialize(objectResult.Value, _jsonSerializerOptions);
        var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
        var contentType = objectResult.ContentTypes.FirstOrDefault()?.ToString() ?? "application/json";

        await _idempotencyStore.CompleteAsync(storeKey, responseBody, statusCode, contentType, ttl, CancellationToken.None);
    }
}