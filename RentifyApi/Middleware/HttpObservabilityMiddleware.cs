using System.Buffers;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;
using RentifyApi.Observability;
using RentifyApplication.Constants;
using RentifyApplication.Exceptions;

namespace RentifyApi.Middleware;

public sealed partial class HttpObservabilityMiddleware(RequestDelegate next, ILogger<HttpObservabilityMiddleware> logger, IConfiguration configuration)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExcluded(context.Request.Path))
        {
            await next(context);
            return;
        }

        var timer = Stopwatch.StartNew();
        var maxBodySize = Math.Clamp(configuration.GetValue("Observability:Http:MaxBodySizeBytes", 16_384), 1, 65_536);
        string? requestBody = null;
        string? responseBody = null;
        ResponseCaptureStream? captureStream = null;
        Exception? observedException = null;
        var originalBody = context.Response.Body;
        var originalBodyFeature = context.Features.Get<IHttpResponseBodyFeature>();

        if (configuration.GetValue("Observability:Http:LogRequestBody", false))
            requestBody = await TryCaptureRequestBodyAsync(context, maxBodySize);

        if (configuration.GetValue("Observability:Http:LogResponseBody", false))
        {
            try
            {
                captureStream = new ResponseCaptureStream(originalBody, maxBodySize, () => IsSafeJsonContentType(context.Response.ContentType) && !HasNonIdentityContentEncoding(context.Response.Headers) && !IsAttachment(context.Response.Headers));
                context.Response.Body = captureStream;
            }
            catch
            {
                captureStream = null;
                RestoreBody(context, originalBody, originalBodyFeature);
            }
        }

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            observedException = exception;
            throw;
        }
        finally
        {
            timer.Stop();

            try
            {
                if (captureStream is not null)
                {
                    RestoreBody(context, originalBody, originalBodyFeature);
                    responseBody = SafeJsonBody.Format(captureStream.CapturedBytes, captureStream.IsTruncated);
                    captureStream.Dispose();
                }
            }
            catch
            {
                RestoreBody(context, originalBody, originalBodyFeature);
                responseBody = null;
            }

            try
            {
                WriteLog(context, timer, requestBody, responseBody, observedException);
            }
            catch
            {
            }
        }
    }

    private void WriteLog(HttpContext context, Stopwatch timer, string? requestBody, string? responseBody, Exception? observedException)
    {
        var statusCode = context.Response.StatusCode;
        var exception = context.Items.TryGetValue(ProjectConstants.ExceptionItem, out var handled) ? handled as Exception : observedException;
        var responseType = exception is not null ? ClassifyException(exception) : ClassifyStatusCode(statusCode);
        var level = statusCode >= 500 ? LogLevel.Error : statusCode >= 400 ? LogLevel.Warning : LogLevel.Information;
        var operation = context.GetEndpoint()?.DisplayName ?? $"{context.Request.Method} {context.Request.Path}";

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["TraceId"] = ObservabilityKeys.GetTraceId(context),
            ["RequestType"] = "Http",
            ["Operation"] = operation,
            ["ResponseType"] = responseType,
            ["UserId"] = GetUserId(context) ?? "anonymous",
            ["RequestBody"] = requestBody,
            ["ResponseBody"] = responseBody,
            ["ExceptionType"] = exception?.GetType().FullName,
            ["ErrorMessage"] = SafeErrorMessage(exception?.Message)
        }))
        {
            logger.Log(level, "HTTP {HttpMethod} {Path} -> {StatusCode} in {DurationMs}ms", context.Request.Method, context.Request.Path.Value, statusCode, timer.ElapsedMilliseconds);
        }
    }

    private static string? GetUserId(HttpContext context) =>
        context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? context.User.FindFirst("sub")?.Value;

    private static void RestoreBody(HttpContext context, Stream originalBody, IHttpResponseBodyFeature? originalFeature)
    {
        if (originalFeature is not null)
            context.Features.Set(originalFeature);
        else
            context.Response.Body = originalBody;
    }

    private static bool IsExcluded(PathString path) =>
        path.StartsWithSegments("/health") || path.StartsWithSegments("/swagger");

    private static string ClassifyException(Exception exception) => exception switch
    {
        ValidationFailedException => "ValidationError",
        LlmServiceException => "OpenAIError",
        DatabaseUniqueConstraintException => "DbError",
        BusinessException => "BusinessError",
        OperationCanceledException => "ClientCancelled",
        _ => "UnhandledError"
    };

    private static string ClassifyStatusCode(int statusCode) => statusCode switch
    {
        >= 500 => "UnhandledError",
        StatusCodes.Status400BadRequest or StatusCodes.Status422UnprocessableEntity => "ValidationError",
        >= 400 => "ClientError",
        _ => "Success"
    };

    private static async Task<string?> TryCaptureRequestBodyAsync(HttpContext context, int maxBytes)
    {
        var request = context.Request;

        if (HttpMethods.IsGet(request.Method) || !IsSafeJsonContentType(request.ContentType) || request.ContentLength == 0 || HasNonIdentityContentEncoding(request.Headers))
            return null;

        long originalPosition = 0;
        var canRestore = false;
        byte[]? buffer = null;

        try
        {
            request.EnableBuffering();

            if (request.Body.CanSeek)
            {
                originalPosition = request.Body.Position;
                canRestore = true;
            }

            buffer = ArrayPool<byte>.Shared.Rent(maxBytes + 1);
            var total = 0;

            while (total < maxBytes + 1)
            {
                var read = await request.Body.ReadAsync(buffer.AsMemory(total, maxBytes + 1 - total), context.RequestAborted);

                if (read == 0)
                    break;

                total += read;
            }

            var truncated = total > maxBytes;
            var capturedCount = Math.Min(total, maxBytes);

            return capturedCount == 0 ? null : SafeJsonBody.Format(buffer.AsSpan(0, capturedCount), truncated);
        }
        catch
        {
            return null;
        }
        finally
        {
            if (canRestore)
            {
                try
                {
                    request.Body.Position = originalPosition;
                }
                catch
                {
                }
            }

            if (buffer is not null)
                ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
        }
    }

    private static bool IsSafeJsonContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            return false;

        var mediaType = contentType.Split(';', 2)[0].Trim();

        return mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) || mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasNonIdentityContentEncoding(IHeaderDictionary headers) =>
        headers.TryGetValue("Content-Encoding", out var encoding) && !StringValues.IsNullOrEmpty(encoding) && !encoding.ToString().Equals("identity", StringComparison.OrdinalIgnoreCase);

    private static bool IsAttachment(IHeaderDictionary headers) =>
        headers.TryGetValue("Content-Disposition", out var disposition) && disposition.ToString().Contains("attachment", StringComparison.OrdinalIgnoreCase);

    internal static string? SafeErrorMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        try
        {
            var safe = message.Length > 4_096 ? message[..4_096] : message;
            safe = SensitiveAssignmentRegex().Replace(safe, "$1$2[REDACTED]");
            safe = BearerTokenRegex().Replace(safe, "Bearer [REDACTED]");
            safe = JwtRegex().Replace(safe, "[REDACTED_JWT]");
            safe = ApiKeyRegex().Replace(safe, "[REDACTED_API_KEY]");
            safe = EmailRegex().Replace(safe, "[REDACTED_EMAIL]");
            safe = UrlRegex().Replace(safe, match => SafeJsonBody.ContainsSensitiveUrl(match.Value) ? "[REDACTED_URL]" : match.Value);
            return safe.Length > 2_048 ? safe[..2_048] + " …[TRUNCATED]" : safe;
        }
        catch (RegexMatchTimeoutException)
        {
            return "[ERROR MESSAGE OMITTED: REDACTION TIMEOUT]";
        }
    }

    [GeneratedRegex("(password|passwd|pwd|token|secret|api[\\s_-]?key|authorization(?:\\s+header)?|cookie|client[\\s_-]?secret)(\\s*[=:]\\s*)(?:Bearer\\s+[^\\s,;]+|[^\\s,;]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex SensitiveAssignmentRegex();

    [GeneratedRegex("Bearer\\s+[A-Za-z0-9._~+/-]+=*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex BearerTokenRegex();

    [GeneratedRegex("eyJ[A-Za-z0-9_-]+\\.[A-Za-z0-9_-]+\\.[A-Za-z0-9_-]*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex JwtRegex();

    [GeneratedRegex("sk-[A-Za-z0-9_-]{12,}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex ApiKeyRegex();

    [GeneratedRegex("[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\\.[A-Za-z]{2,}", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex EmailRegex();

    [GeneratedRegex("https?://[^\\s\\\"'<>]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 100)]
    private static partial Regex UrlRegex();
}