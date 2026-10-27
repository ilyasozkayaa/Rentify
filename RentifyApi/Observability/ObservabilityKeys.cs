using System.Diagnostics;

namespace RentifyApi.Observability;

internal static class ObservabilityKeys
{
    public static string GetTraceId(HttpContext context) => Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
}