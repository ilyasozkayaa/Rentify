using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace RentifyInfrastructure.Observability;

public static class ObservabilityLog
{
    public static string TraceId => Activity.Current?.TraceId.ToString() ?? "background";

    public static void Completed(ILogger logger, string requestType, string operation, long durationMs, string responseType, Exception? exception = null, params (string Name, object? Value)[] fields)
    {
        var properties = new List<object?> { TraceId, requestType, operation, durationMs, responseType };
        properties.AddRange(fields.Select(field => field.Value));
        var names = string.Join(" ", fields.Select(field => $"{field.Name}: {{{field.Name}}}"));
        var message = $"{{TraceId}} {{RequestType}} operation {{Operation}} completed in {{DurationMs}}ms with {{ResponseType}} {names}";

        if (exception is not null)
            logger.LogError(exception, message, properties.ToArray());
        else if (responseType == "Success")
            logger.LogInformation(message, properties.ToArray());
        else
            logger.LogWarning(message, properties.ToArray());
    }
}