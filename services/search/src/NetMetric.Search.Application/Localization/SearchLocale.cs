// <copyright file="SearchLocale.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Localization;

public static class SearchLocale
{
    private static readonly IReadOnlyDictionary<string, string> SupportedLocaleByName =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            [SearchDocumentLocales.EnglishUnitedStates] = SearchDocumentLocales.EnglishUnitedStates,
            [SearchDocumentLocales.TurkishTurkey] = SearchDocumentLocales.TurkishTurkey
        };

    private static readonly IReadOnlyDictionary<string, string> SupportedLocaleByLanguage =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["en"] = SearchDocumentLocales.EnglishUnitedStates,
            ["tr"] = SearchDocumentLocales.TurkishTurkey
        };

    public static IReadOnlyCollection<string> SupportedStaticLocales { get; } =
    [
        SearchDocumentLocales.EnglishUnitedStates,
        SearchDocumentLocales.TurkishTurkey
    ];

    public static string DefaultLocale => SearchDocumentLocales.EnglishUnitedStates;

    public static string? CanonicalizeRequestedLocale(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return CanonicalizeSupportedLocale(value) ?? DefaultLocale;
    }

    public static string CanonicalizeStaticLocale(string? value) =>
        string.IsNullOrWhiteSpace(value) ? DefaultLocale : CanonicalizeSupportedLocale(value) ?? DefaultLocale;

    private static string? CanonicalizeSupportedLocale(string value)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            return null;
        }

        if (SupportedLocaleByName.TryGetValue(normalized, out var supported))
        {
            return supported;
        }

        var language = normalized.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (language is not null && SupportedLocaleByLanguage.TryGetValue(language, out supported))
        {
            return supported;
        }

        return null;
    }

    private static string? Normalize(string value)
    {
        var trimmed = value.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return null;
        }

        if (string.Equals(trimmed, SearchDocumentLocales.Neutral, StringComparison.OrdinalIgnoreCase))
        {
            return SearchDocumentLocales.Neutral;
        }

        try
        {
            return CultureInfo.GetCultureInfo(trimmed).Name;
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }
}
