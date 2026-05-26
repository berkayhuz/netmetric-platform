// <copyright file="MarketingUtilities.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public static class MarketingUtilities
{
    public static string HashEmail(string emailAddress)
    {
        var normalized = NormalizeEmail(emailAddress);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized))).ToLowerInvariant();
    }

    public static string NormalizeEmail(string emailAddress)
        => emailAddress.Trim().ToLowerInvariant();

    public static string Sanitize(string? message)
        => string.IsNullOrWhiteSpace(message) ? "Marketing automation operation failed." : message.Trim()[..Math.Min(message.Trim().Length, 1000)];

    public static string? ReadString(string json, string field)
    {
        try
        {
            using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(json) ? "{}" : json);
            JsonElement current = document.RootElement;
            foreach (var part in field.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(part, out current))
                {
                    return null;
                }
            }

            return current.ValueKind switch
            {
                JsonValueKind.String => current.GetString(),
                JsonValueKind.Number => current.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => current.GetRawText()
            };
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
