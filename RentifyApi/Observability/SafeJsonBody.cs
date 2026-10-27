using RentifyApplication.Constants;
using System.Buffers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace RentifyApi.Observability;

internal static class SafeJsonBody
{
    private static readonly string[] SensitiveFragments =
    [
        "password", "passwd", "pwd", "token", "secret", "apikey", "authorization", "cookie", "bearer", "credential", "privatekey", "jwt"
    ];

    private static readonly HashSet<string> SensitiveExactNames = new(StringComparer.Ordinal)
    {
        "pin", "otp", "cvv", "iban", "tckn", "identitynumber", "email", "phone", "phonenumber"
    };

    private static readonly HashSet<string> AllowedNames = new(StringComparer.Ordinal)
    {
        "prompttokens", "completiontokens", "totaltokens", "maxtokens", "inputtokens", "outputtokens", "maxcompletiontokens", "cachedtokens", "reasoningtokens"
    };

    private static readonly HashSet<string> SensitiveQueryParameters = new(StringComparer.OrdinalIgnoreCase)
    {
        "x-amz-signature", "x-amz-credential", "x-amz-security-token", "x-goog-signature", "x-goog-credential", "signature", "sig", "access_token", "refresh_token", "id_token", "token", "api_key", "apikey", "key", "code", "password"
    };

    public static string? Format(ReadOnlySpan<byte> bytes, bool truncated)
    {
        if (bytes.IsEmpty)
            return null;

        var output = new ArrayBufferWriter<byte>(Math.Min(bytes.Length + 16, 65_536));

        try
        {
            using var writer = new Utf8JsonWriter(output, new JsonWriterOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Indented = false,
                SkipValidation = true
            });

            var reader = new Utf8JsonReader(bytes, isFinalBlock: !truncated, state: default);
            RedactInto(ref reader, writer);
            writer.Flush();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or InvalidOperationException)
        {
            return "[INVALID JSON; CONTENT OMITTED FOR SAFE REDACTION]";
        }

        var result = Encoding.UTF8.GetString(output.WrittenSpan);
        return truncated ? result + " …[TRUNCATED]" : result;
    }

    private static void RedactInto(ref Utf8JsonReader reader, Utf8JsonWriter writer)
    {
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;
                case JsonTokenType.PropertyName:
                    {
                        var name = reader.GetString()!;
                        writer.WritePropertyName(name);

                        if (!IsSensitiveName(name))
                            break;

                        if (!reader.Read())
                        {
                            writer.WriteStringValue(ProjectConstants.Redacted);
                            return;
                        }

                        if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray && !reader.TrySkip())
                        {
                            writer.WriteStringValue(ProjectConstants.Redacted);
                            return;
                        }

                        writer.WriteStringValue(ProjectConstants.Redacted);
                        break;
                    }
                case JsonTokenType.String:
                    {
                        var text = reader.GetString()!;
                        writer.WriteStringValue(ContainsSensitiveUrl(text) ? ProjectConstants.Redacted : text);
                        break;
                    }
                case JsonTokenType.Number:
                    writer.WriteRawValue(reader.ValueSpan, skipInputValidation: true);
                    break;
                case JsonTokenType.True:
                    writer.WriteBooleanValue(true);
                    break;
                case JsonTokenType.False:
                    writer.WriteBooleanValue(false);
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
            }
        }
    }

    public static bool IsSensitiveName(string name)
    {
        var normalized = name.Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

        if (AllowedNames.Contains(normalized))
            return false;

        if (SensitiveExactNames.Contains(normalized))
            return true;

        foreach (var fragment in SensitiveFragments)
        {
            if (normalized.Contains(fragment, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static bool ContainsSensitiveUrl(string value)
    {
        var queryStart = value.IndexOf('?', StringComparison.Ordinal);

        if (queryStart < 0 || queryStart == value.Length - 1)
            return false;

        var query = value[(queryStart + 1)..];
        var fragmentStart = query.IndexOf('#', StringComparison.Ordinal);

        if (fragmentStart >= 0)
            query = query[..fragmentStart];

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var equals = pair.IndexOf('=', StringComparison.Ordinal);
            var key = equals >= 0 ? pair[..equals] : pair;

            if (SensitiveQueryParameters.Contains(key))
                return true;
        }

        return false;
    }
}