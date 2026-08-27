using Microsoft.AspNetCore.Mvc;
using RentifyApplication.Exceptions;

namespace RentifyApi.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationFailedException ex)
        {
            _logger.LogWarning( "Validation failed for request {Path}", context.Request.Path);
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (LlmServiceException ex)
        {
            _logger.LogError(ex, "LLM service failure for request {Path}", context.Request.Path);
            await HandleLlmExceptionAsync(context, ex);
        }
        catch (OperationCanceledException) when (
            context.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled by the client. Path: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception for request {Path}", context.Request.Path);
            await HandleExceptionAsync(context);
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationFailedException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://rentify.dev/errors/validation",
            Title = "Validation failed",
            Status = StatusCodes.Status400BadRequest,
            Detail = "One or more validation errors occurred."
        };

        problemDetails.Extensions["code"] = "VALIDATION_ERROR";
        problemDetails.Extensions["errors"] = exception.Errors;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await WriteResponseAsync(context, problemDetails);
    }

    private static async Task HandleLlmExceptionAsync(HttpContext context, LlmServiceException exception)
    {
        var problemDetails = new ProblemDetails
        {
            Type = $"https://rentify.dev/errors/{exception.Code.ToLowerInvariant()}",
            Title = exception.Message,
            Status = exception.StatusCode,
            Detail = "The search service could not process the request."
        };

        problemDetails.Extensions["code"] = exception.Code;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await WriteResponseAsync(context, problemDetails);
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://rentify.dev/errors/internal-server-error",
            Title = "An unexpected error occurred.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "An unexpected error occurred while processing the request."
        };

        problemDetails.Extensions["code"] = "INTERNAL_SERVER_ERROR";
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await WriteResponseAsync(context, problemDetails);
    }

    private static async Task WriteResponseAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.StatusCode = problemDetails.Status ?? 500;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
