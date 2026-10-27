using System.Data;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using RentifyInfrastructure.Observability;

namespace RentifyInfrastructure.Persistence;

public sealed class ObservabilityDbCommandInterceptor(ILogger<ObservabilityDbCommandInterceptor> logger) : DbCommandInterceptor
{
    private const int MaxParameterStringLength = 512;

    private static string Operation(DbCommand command) => command.CommandText.TrimStart().Split(' ', '\r', '\n', '\t')[0] switch
    {
        "SELECT" => "Query",
        "INSERT" => "Insert",
        "UPDATE" => "Update",
        "DELETE" => "Delete",
        "CREATE" => "SchemaCommand",
        _ => "Command"
    };

    private static IReadOnlyDictionary<string, object?> GetSafeParameters(DbCommand command)
    {
        var parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (DbParameter parameter in command.Parameters)
        {
            var name = parameter.ParameterName;
            if (IsSensitiveParameter(name))
            {
                parameters[name] = "[REDACTED]";
                continue;
            }

            if (parameter.DbType == DbType.Binary)
                continue;

            var value = parameter.Value;
            if (value is null or DBNull)
            {
                parameters[name] = null;
                continue;
            }

            if (value is byte[] or ReadOnlyMemory<byte> or Memory<byte> or Stream)
                continue;

            if (value is string text)
            {
                const string truncatedSuffix = "…[TRUNCATED]";
                parameters[name] = text.Length > MaxParameterStringLength
                    ? text[..(MaxParameterStringLength - truncatedSuffix.Length)] + truncatedSuffix
                    : text;
                continue;
            }

            if (value is bool or byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal or Guid or DateTime or DateTimeOffset or DateOnly or TimeOnly or TimeSpan)
                parameters[name] = value;
            else if (value.GetType().IsEnum)
                parameters[name] = Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        return parameters;
    }

    private static bool IsSensitiveParameter(string name)
    {
        var normalized = name.Replace("@", string.Empty, StringComparison.Ordinal)
            .Replace(":", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);
        return normalized.Contains("password", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("token", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("secret", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("apikey", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("authorization", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("cookie", StringComparison.OrdinalIgnoreCase);
    }

    private static void LogSuccess(DbCommand command, CommandEventData eventData, ILogger logger) =>
        ObservabilityLog.Completed(logger, "Database", Operation(command), (long)(DateTimeOffset.UtcNow - eventData.StartTime).TotalMilliseconds, "Success", null,
            ("CommandText", command.CommandText), ("Parameters", GetSafeParameters(command)));

    private static void LogError(DbCommand command, CommandErrorEventData eventData, ILogger logger) =>
        ObservabilityLog.Completed(logger, "Database", Operation(command), (long)(DateTimeOffset.UtcNow - eventData.StartTime).TotalMilliseconds, "DbError", eventData.Exception,
            ("CommandText", command.CommandText),
            ("ExceptionType", eventData.Exception.GetType().FullName),
            ("ErrorMessage", eventData.Exception.Message),
            ("Parameters", GetSafeParameters(command)));

    public override DbDataReader ReaderExecuted(DbCommand command, CommandExecutedEventData eventData, DbDataReader result) { LogSuccess(command, eventData, logger); return result; }
    public override ValueTask<DbDataReader> ReaderExecutedAsync(DbCommand command, CommandExecutedEventData eventData, DbDataReader result, CancellationToken cancellationToken = default) { LogSuccess(command, eventData, logger); return ValueTask.FromResult(result); }
    public override int NonQueryExecuted(DbCommand command, CommandExecutedEventData eventData, int result) { LogSuccess(command, eventData, logger); return result; }
    public override ValueTask<int> NonQueryExecutedAsync(DbCommand command, CommandExecutedEventData eventData, int result, CancellationToken cancellationToken = default) { LogSuccess(command, eventData, logger); return ValueTask.FromResult(result); }
    public override object? ScalarExecuted(DbCommand command, CommandExecutedEventData eventData, object? result) { LogSuccess(command, eventData, logger); return result; }
    public override ValueTask<object?> ScalarExecutedAsync(DbCommand command, CommandExecutedEventData eventData, object? result, CancellationToken cancellationToken = default) { LogSuccess(command, eventData, logger); return ValueTask.FromResult(result); }
    public override void CommandFailed(DbCommand command, CommandErrorEventData eventData) => LogError(command, eventData, logger);
    public override Task CommandFailedAsync(DbCommand command, CommandErrorEventData eventData, CancellationToken cancellationToken = default) { LogError(command, eventData, logger); return Task.CompletedTask; }
}