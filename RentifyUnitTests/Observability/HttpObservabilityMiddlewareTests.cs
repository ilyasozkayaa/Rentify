using System.Collections.Concurrent;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentifyApi.Middleware;
using RentifyApplication.Exceptions;
using RentifyApplication.Exceptions.Enums;

namespace RentifyUnitTests.Observability;

public sealed class HttpObservabilityMiddlewareTests
{
    [Fact]
    public async Task CapturesAndRedactsJsonBodies_AndPreservesRequestAndResponseStreams()
    {
        const string requestJson = "{\"email\":\"test@test.com\",\"password\":\"123456\",\"refreshToken\":\"refresh-value\",\"lastName\":\"Özkaya\"}";
        const string responseJson = "{\"firstName\":\"İlyas\",\"accessToken\":\"access-value\",\"userId\":7}";
        var (context, provider, middleware) = CreateContext("POST", "application/json", requestJson);
        var downstreamRead = string.Empty;
        middleware = new HttpObservabilityMiddleware(async httpContext =>
        {
            using var reader = new StreamReader(httpContext.Request.Body, Encoding.UTF8, leaveOpen: true);
            downstreamRead = await reader.ReadToEndAsync();
            httpContext.Response.ContentType = "application/json; charset=utf-8";
            await httpContext.Response.WriteAsync(responseJson);
        }, provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration());

        await middleware.InvokeAsync(context);

        Assert.Equal(requestJson, downstreamRead);
        Assert.Equal(responseJson, Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()));
        var log = Assert.Single(provider.Events);
        Assert.Equal("trace-http-test", log.Properties["TraceId"]);
        Assert.Equal("Http", log.Properties["RequestType"]);
        Assert.Contains("\"password\":\"[REDACTED]\"", Assert.IsType<string>(log.Properties["RequestBody"]));
        Assert.Contains("\"refreshToken\":\"[REDACTED]\"", Assert.IsType<string>(log.Properties["RequestBody"]));
        Assert.Contains("Özkaya", Assert.IsType<string>(log.Properties["RequestBody"]));
        Assert.DoesNotContain("123456", Assert.IsType<string>(log.Properties["RequestBody"]));
        Assert.Contains("\"accessToken\":\"[REDACTED]\"", Assert.IsType<string>(log.Properties["ResponseBody"]));
        Assert.Contains("İlyas", Assert.IsType<string>(log.Properties["ResponseBody"]));
        Assert.DoesNotContain("access-value", Assert.IsType<string>(log.Properties["ResponseBody"]));
    }

    [Fact]
    public async Task DoesNotCaptureMultipartOrGetRequestBodies()
    {
        foreach (var (method, contentType) in new[]
        {
            ("POST", "multipart/form-data; boundary=test"),
            ("GET", "application/json")
        })
        {
            var (context, provider, middleware) = CreateContext(method, contentType, "{\"password\":\"secret\"}");
            await middleware.InvokeAsync(context);
            var log = Assert.Single(provider.Events);
            Assert.Null(log.Properties["RequestBody"]);
        }
    }

    [Fact]
    public async Task ClassifiesStoredExceptionsByType_AndKeepsTraceCorrelation()
    {
        var cases = new (Exception Exception, string ResponseType)[]
        {
            (new ValidationFailedException(new Dictionary<string, string[]>()), "ValidationError"),
            (new LlmServiceException("LLM_FAILURE", 500, "LLM failed"), "OpenAIError"),
            (new DatabaseUniqueConstraintException("unique_email", new Exception()), "DbError"),
            (new BusinessException("Business failure", BusinessErrorCode.InvalidCredentials), "BusinessError"),
            (new Exception("Unhandled failure"), "UnhandledError")
        };

        foreach (var (exception, expectedResponseType) in cases)
        {
            var (context, provider, _) = CreateContext("GET", "application/json", string.Empty);
            var middleware = new HttpObservabilityMiddleware(httpContext =>
            {
                // ExceptionHandlingMiddleware stores handled exceptions at this key.
                httpContext.Items["Rentify.Observability.Exception"] = exception;
                return Task.CompletedTask;
            }, provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration());

            await middleware.InvokeAsync(context);

            var log = Assert.Single(provider.Events);
            Assert.Equal(expectedResponseType, log.Properties["ResponseType"]);
            Assert.Equal("trace-http-test", log.Properties["TraceId"]);
        }
    }

    [Fact]
    public async Task UsesStatusCodeClassificationWhenNoExceptionIsAvailable()
    {
        foreach (var (statusCode, expectedResponseType) in new[]
        {
            (StatusCodes.Status302Found, "Success"),
            (StatusCodes.Status400BadRequest, "ValidationError"),
            (StatusCodes.Status422UnprocessableEntity, "ValidationError"),
            (StatusCodes.Status401Unauthorized, "ClientError"),
            (StatusCodes.Status500InternalServerError, "UnhandledError")
        })
        {
            var (context, provider, _) = CreateContext("GET", "application/json", string.Empty);
            context.Response.StatusCode = statusCode;
            var middleware = new HttpObservabilityMiddleware(_ => Task.CompletedTask,
                provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration());

            await middleware.InvokeAsync(context);

            var log = Assert.Single(provider.Events);
            Assert.Equal(expectedResponseType, log.Properties["ResponseType"]);
        }
    }

    [Fact]
    public async Task TruncatesLargeBodiesAndOmitsBinaryResponseBodiesWithoutChangingResponse()
    {
        var largeRequest = "{\"note\":\"" + new string('a', 90) + "\",\"password\":\"secret-value\"}";
        var (context, provider, middleware) = CreateContext("POST", "application/json", largeRequest, maxBytes: 32);
        var binaryResponse = new byte[] { 0, 1, 2, 3, 255 };
        middleware = new HttpObservabilityMiddleware(async httpContext =>
        {
            httpContext.Response.ContentType = "image/png";
            await httpContext.Response.Body.WriteAsync(binaryResponse);
        }, provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration(maxBytes: 32));

        await middleware.InvokeAsync(context);

        var log = Assert.Single(provider.Events);
        var loggedRequest = Assert.IsType<string>(log.Properties["RequestBody"]);
        Assert.Contains("TRUNCATED", loggedRequest);
        Assert.DoesNotContain("secret-value", loggedRequest);
        Assert.Null(log.Properties["ResponseBody"]);
        Assert.Equal(binaryResponse, ((MemoryStream)context.Response.Body).ToArray());
    }

    [Fact]
    public async Task RedactsSecretsEvenWhenRequestJsonIsTruncated_AndBoundsResponseCapture()
    {
        const string secret = "do-not-log-this-secret-value";
        var requestJson = "{\"password\":\"" + secret + new string('x', 80) + "\"}";
        var responseJson = "{\"message\":\"" + new string('r', 100) + "\"}";
        var (context, provider, _) = CreateContext("POST", "application/json", requestJson, maxBytes: 32);
        var middleware = new HttpObservabilityMiddleware(async httpContext =>
        {
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsync(responseJson);
        }, provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration(maxBytes: 32));

        await middleware.InvokeAsync(context);

        var log = Assert.Single(provider.Events);
        var loggedRequest = Assert.IsType<string>(log.Properties["RequestBody"]);
        Assert.Contains("[REDACTED]", loggedRequest);
        Assert.Contains("TRUNCATED", loggedRequest);
        Assert.DoesNotContain(secret, loggedRequest);
        var loggedResponse = Assert.IsType<string>(log.Properties["ResponseBody"]);
        Assert.Contains("TRUNCATED", loggedResponse);
        Assert.True(loggedResponse.Length < responseJson.Length);
        Assert.Equal(responseJson, Encoding.UTF8.GetString(((MemoryStream)context.Response.Body).ToArray()));
    }

    private static (DefaultHttpContext Context, CaptureLoggerProvider Provider, HttpObservabilityMiddleware Middleware) CreateContext(
        string method, string contentType, string body, int maxBytes = 16_384)
    {
        var provider = new CaptureLoggerProvider();
        var context = new DefaultHttpContext();
        context.TraceIdentifier = "trace-http-test";
        context.Request.Method = method;
        context.Request.Path = "/api/Auth/login";
        context.Request.ContentType = contentType;
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.ContentLength = context.Request.Body.Length;
        context.Response.Body = new MemoryStream();
        var middleware = new HttpObservabilityMiddleware(_ => Task.CompletedTask,
            provider.CreateLogger<HttpObservabilityMiddleware>(), Configuration(maxBytes));
        return (context, provider, middleware);
    }

    private static IConfiguration Configuration(int maxBytes = 16_384) => new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Observability:Http:LogRequestBody"] = "true",
            ["Observability:Http:LogResponseBody"] = "true",
            ["Observability:Http:MaxBodySizeBytes"] = maxBytes.ToString()
        })
        .Build();

    private sealed record CapturedEvent(string Message, IReadOnlyDictionary<string, object?> Properties);

    private sealed class CaptureLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedEvent> _events = new();
        public IReadOnlyCollection<CapturedEvent> Events => _events.ToArray();
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(_events);
        public ILogger<T> CreateLogger<T>() => new TypedCaptureLogger<T>(_events);
        public void Dispose() { }

        private class CaptureLogger(ConcurrentQueue<CapturedEvent> events) : ILogger
        {
            private readonly AsyncLocal<IReadOnlyDictionary<string, object?>?> _scope = new();

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                var previous = _scope.Value;
                _scope.Value = state is IEnumerable<KeyValuePair<string, object?>> values
                    ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                    : null;
                return new Scope(() => _scope.Value = previous);
            }

            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                    ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                    : new Dictionary<string, object?>();
                if (_scope.Value is { } scope)
                    foreach (var (key, value) in scope)
                        properties.TryAdd(key, value);
                events.Enqueue(new CapturedEvent(formatter(state, exception), properties));
            }

            private sealed class Scope(Action dispose) : IDisposable
            {
                public void Dispose() => dispose();
            }
        }

        private sealed class TypedCaptureLogger<T>(ConcurrentQueue<CapturedEvent> events) : CaptureLogger(events), ILogger<T> { }
    }
}
