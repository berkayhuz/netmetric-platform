// <copyright file="ImportValueParser.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using NetMetric.CRM.Types;


namespace NetMetric.CRM.CustomerManagement.Application.Common;

internal static class ImportValueParser
{
    public static string? Get(
        IReadOnlyDictionary<string, string?> row,
        params string[] keys)
    {
        foreach (var key in keys)
        {
            if (row.TryGetValue(NormalizeKey(key), out var value) && !string.IsNullOrWhiteSpace(value))
                return value.Trim();
        }

        return null;
    }

    public static Guid? ParseGuid(string? value)
        => Guid.TryParse(value, out var parsed) ? parsed : null;

    public static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed)
            ? parsed
            : DateTime.TryParse(value, CultureInfo.GetCultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out parsed)
                ? parsed.ToUniversalTime()
                : null;
    }

    public static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var invariant))
            return invariant;

        return decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("tr-TR"), out var tr)
            ? tr
            : null;
    }

    public static bool? ParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim().ToLowerInvariant() switch
        {
            "1" or "true" or "yes" or "y" or "evet" => true,
            "0" or "false" or "no" or "n" or "hayır" or "hayir" => false,
            _ => null
        };
    }

    public static TEnum? ParseEnum<TEnum>(string? value)
        where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return Enum.TryParse<TEnum>(value.Trim(), ignoreCase: true, out var parsed)
            ? parsed
            : null;
    }

    public static string NormalizeKey(string key)
        => key.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal);

    public static GenderType ParseGenderOrDefault(string? value)
        => ParseEnum<GenderType>(value) ?? GenderType.Unknown;
}
