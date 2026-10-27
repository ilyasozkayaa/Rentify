using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using RentifyInfrastructure.Persistence;

namespace RentifyUnitTests.Observability;

public sealed class ObservabilityDbCommandInterceptorTests
{
    [Fact]
    public void LogsParameterizedCommandAndRedactedStructuredParametersWithCurrentTraceId()
    {
        using var activity = new Activity("login-request");
        activity.SetIdFormat(ActivityIdFormat.W3C);
        activity.Start();

        using var connection = new NpgsqlConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM users WHERE email = @email AND password_hash = @passwordHash";
        command.Parameters.AddWithValue("@email", "ilyas@gmail.com");
        command.Parameters.AddWithValue("@passwordHash", "stored-hash");
        command.Parameters.AddWithValue("@accessToken", "access-token-value");
        command.Parameters.AddWithValue("@refresh_token", "refresh-token-value");
        command.Parameters.AddWithValue("@apiKey", "openai-api-key-value");
        command.Parameters.AddWithValue("@clientSecret", "client-secret-value");
        command.Parameters.AddWithValue("@authorization", "Bearer authorization-value");
        command.Parameters.AddWithValue("@cookie", "session-cookie-value");
        command.Parameters.AddWithValue("@secret", "minio-secret-value");
        command.Parameters.AddWithValue("@profileBlob", new byte[] { 1, 2, 3 });
        command.Parameters.AddWithValue("@longNote", new string('x', 800));

        var loggerProvider = new CaptureLoggerProvider();
        var interceptor = new ObservabilityDbCommandInterceptor(loggerProvider.CreateLogger<ObservabilityDbCommandInterceptor>());
        var startTime = DateTimeOffset.UtcNow.AddMilliseconds(-12);
        var eventData = new CommandExecutedEventData(
            null!, (_, _) => string.Empty, connection, command, command.CommandText, null,
            DbCommandMethod.ExecuteReader, Guid.NewGuid(), Guid.NewGuid(), null,
            async: false, logParameterValues: false, startTime, TimeSpan.FromMilliseconds(12), CommandSource.LinqQuery);

        interceptor.ReaderExecuted(command, eventData, null!);

        var entry = Assert.Single(loggerProvider.Events);
        Assert.Equal(Activity.Current?.TraceId.ToString(), entry.Properties["TraceId"]);
        Assert.Equal("Database", entry.Properties["RequestType"]);
        Assert.Equal("Query", entry.Properties["Operation"]);
        Assert.Equal(command.CommandText, entry.Properties["CommandText"]);
        Assert.Contains("@email", Assert.IsType<string>(entry.Properties["CommandText"]));
        Assert.DoesNotContain("ilyas@gmail.com", Assert.IsType<string>(entry.Properties["CommandText"]));

        var parameters = Assert.IsAssignableFrom<IReadOnlyDictionary<string, object?>>(entry.Properties["Parameters"]);
        Assert.Equal("ilyas@gmail.com", parameters["@email"]);
        Assert.Equal("[REDACTED]", parameters["@passwordHash"]);
        Assert.Equal("[REDACTED]", parameters["@accessToken"]);
        Assert.Equal("[REDACTED]", parameters["@refresh_token"]);
        Assert.Equal("[REDACTED]", parameters["@apiKey"]);
        Assert.Equal("[REDACTED]", parameters["@clientSecret"]);
        Assert.Equal("[REDACTED]", parameters["@authorization"]);
        Assert.Equal("[REDACTED]", parameters["@cookie"]);
        Assert.Equal("[REDACTED]", parameters["@secret"]);
        Assert.DoesNotContain("@profileBlob", parameters.Keys);
        Assert.Contains("[TRUNCATED]", Assert.IsType<string>(parameters["@longNote"]));
        activity.Stop();
    }

    private sealed record CapturedEvent(IReadOnlyDictionary<string, object?> Properties);

    private sealed class CaptureLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<CapturedEvent> _events = new();
        public IReadOnlyCollection<CapturedEvent> Events => _events.ToArray();
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(_events);
        public ILogger<T> CreateLogger<T>() => new CaptureLogger<T>(_events);
        public void Dispose() { }

        private class CaptureLogger(ConcurrentQueue<CapturedEvent> events) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var properties = state is IEnumerable<KeyValuePair<string, object?>> values
                    ? values.ToDictionary(pair => pair.Key, pair => pair.Value)
                    : new Dictionary<string, object?>();
                events.Enqueue(new CapturedEvent(properties));
            }
        }

        private sealed class CaptureLogger<T>(ConcurrentQueue<CapturedEvent> events) : CaptureLogger(events), ILogger<T> { }
    }
}
