// <copyright file="WorkflowPayloadRedactor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using System.Text.RegularExpressions;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed partial class WorkflowPayloadRedactor : IWorkflowPayloadRedactor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public string RedactJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return "{}";
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var redacted = RedactElement(document.RootElement);
            return JsonSerializer.Serialize(redacted, SerializerOptions);
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { raw = RedactText(json) }, SerializerOptions);
        }
    }

    public string RedactText(string? text, int maxLength = 1000)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var sanitized = SecretLikePattern().Replace(text, "$1=[redacted]");
        sanitized = BearerPattern().Replace(sanitized, "Bearer [redacted]");
        return sanitized.Length <= maxLength ? sanitized : sanitized[..maxLength];
    }

    private static object? RedactElement(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(
                property => property.Name,
                property => IsSensitiveName(property.Name) ? "[redacted]" : RedactElement(property.Value)),
            JsonValueKind.Array => element.EnumerateArray().Select(RedactElement).ToList(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when element.TryGetDecimal(out var decimalValue) => decimalValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }

    private static bool IsSensitiveName(string name)
        => name.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("token", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("password", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("apiKey", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("authorization", StringComparison.OrdinalIgnoreCase) ||
           name.Contains("signature", StringComparison.OrdinalIgnoreCase);

    [GeneratedRegex("(?i)(secret|token|password|api[_-]?key|authorization|signature)\\s*[:=]\\s*[^\\s,;]+")]
    private static partial Regex SecretLikePattern();

    [GeneratedRegex("(?i)Bearer\\s+[A-Za-z0-9._~+/=-]+")]
    private static partial Regex BearerPattern();
}
