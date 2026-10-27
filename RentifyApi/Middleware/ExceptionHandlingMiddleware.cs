using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RentifyApi.Observability;
using RentifyApplication.Constants;
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
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            context.Items[ProjectConstants.ExceptionItem] = new OperationCanceledException();
            _logger.LogInformation("Request cancelled by client. Path: {Path}", context.Request.Path);
        }
        catch (Exception ex)
        {
            context.Items[ProjectConstants.ExceptionItem] = ex;
            LogException(ex, context);

            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started; cannot write problem details. Path: {Path}", context.Request.Path);
                context.Abort();
                return;
            }

            var problem = ex switch
            {
                ValidationFailedException validationException => BuildValidation(context, validationException),
                LlmServiceException llmException => BuildLlm(context, llmException),
                BusinessException businessException => BuildBusiness(context, businessException),
                DatabaseUniqueConstraintException => BuildConflict(context),
                _ => BuildInternal(context)
            };

            await WriteResponseAsync(context, problem);
        }
    }

    private void LogException(Exception ex, HttpContext context)
    {
        var level = ex switch
        {
            ValidationFailedException => LogLevel.Warning,
            LlmServiceException => LogLevel.Error,
            BusinessException or DatabaseUniqueConstraintException => LogLevel.Information,
            _ => LogLevel.Error
        };

        _logger.Log(level, "{Kind} for request {Path}. TraceId: {TraceId}, ExceptionType: {ExceptionType}, ErrorMessage: {ErrorMessage}, StackTrace: {StackTrace}", ex.GetType().Name, context.Request.Path, ObservabilityKeys.GetTraceId(context), ex.GetType().FullName, HttpObservabilityMiddleware.SafeErrorMessage(ex.Message), level == LogLevel.Error ? ex.StackTrace : null);
    }

    private static ProblemDetails BuildValidation(HttpContext context, ValidationFailedException exception)
    {
        var problem = CreateProblem(context, "https://rentify.dev/errors/validation", "Validation failed", StatusCodes.Status400BadRequest, "One or more validation errors occurred.", "VALIDATION_ERROR");
        problem.Extensions["errors"] = exception.Errors;
        return problem;
    }

    private static ProblemDetails BuildLlm(HttpContext context, LlmServiceException exception) =>
        CreateProblem(context, $"https://rentify.dev/errors/{exception.Code.ToLowerInvariant()}", "The search service could not process the request.", exception.StatusCode, "The search service could not process the request.", exception.Code);

    private static ProblemDetails BuildBusiness(HttpContext context, BusinessException exception) =>
        CreateProblem(context, $"https://rentify.dev/errors/{exception.Code.ToString().ToLowerInvariant()}", exception.Message, exception.StatusCode, exception.Message, exception.Code.ToString());

    private static ProblemDetails BuildConflict(HttpContext context) =>
        CreateProblem(context, "https://rentify.dev/errors/unique-constraint-violation", "A conflict occurred.", StatusCodes.Status409Conflict, "A resource with the same unique value already exists.", "UNIQUE_CONSTRAINT_VIOLATION");

    private static ProblemDetails BuildInternal(HttpContext context) =>
        CreateProblem(context, "https://rentify.dev/errors/internal-server-error", "An unexpected error occurred.", StatusCodes.Status500InternalServerError, "An unexpected error occurred while processing the request.", "INTERNAL_SERVER_ERROR");

    private static ProblemDetails CreateProblem(HttpContext context, string type, string title, int status, string detail, string code)
    {
        var problem = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = status,
            Detail = detail,
            Instance = context.Request.Path
        };

        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = ObservabilityKeys.GetTraceId(context);
        return problem;
    }

    private static async Task WriteResponseAsync(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(problemDetails, (JsonSerializerOptions?)null, "application/problem+json");
    }
}