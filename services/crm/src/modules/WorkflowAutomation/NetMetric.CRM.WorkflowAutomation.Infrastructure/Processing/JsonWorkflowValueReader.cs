// <copyright file="JsonWorkflowValueReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Text.Json;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

internal static class JsonWorkflowValueReader
{
    public static JsonElement? TryRead(string payloadJson, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return null;
        }

        using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson);
        var current = document.RootElement.Clone();
        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
            {
                return null;
            }

            current = next.Clone();
        }

        return current;
    }

    public static string? AsString(JsonElement element)
        => element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => null,
            _ => element.GetRawText()
        };

    public static decimal? AsDecimal(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetDecimal(out var number))
        {
            return number;
        }

        return decimal.TryParse(AsString(element), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed) ? parsed : null;
    }

    public static DateTime? AsDateTime(JsonElement element)
        => DateTime.TryParse(AsString(element), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : null;
}
