// <copyright file="AccountOptionsCatalog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using NetMetric.Account.Contracts.Preferences;
using NetMetric.Account.Domain.Preferences;
using NetMetric.Localization;
using PhoneNumbers;

namespace NetMetric.Account.Application.Common;

internal static class AccountOptionsCatalog
{
    private static readonly IReadOnlyCollection<AccountOptionItem> LanguageOptions = CultureInfo
        .GetCultures(CultureTypes.NeutralCultures)
        .Where(culture => !string.IsNullOrWhiteSpace(culture.Name))
        .OrderBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
        .Select(culture =>
        {
            var info = CultureInfo.GetCultureInfo(culture.Name);
            return new AccountOptionItem(info.Name, $"{info.EnglishName} ({info.Name})");
        })
        .ToArray();

    private static readonly IReadOnlyCollection<AccountOptionItem> TimeZoneOptions = TimeZoneInfo.GetSystemTimeZones()
        .Select(zone => new AccountOptionItem(zone.Id, $"{zone.DisplayName} ({zone.Id})"))
        .ToArray();

    private static readonly IReadOnlyCollection<AccountOptionItem> ThemeOptions =
    [
        new("System", "System"),
        new("Dark", "Dark"),
        new("Light", "Light")
    ];

    private static readonly IReadOnlyCollection<AccountOptionItem> DateFormatOptions =
    [
        new("yyyy-MM-dd", "2026-05-15"),
        new("dd/MM/yyyy", "15/05/2026"),
        new("MM/dd/yyyy", "05/15/2026"),
        new("dd.MM.yyyy", "15.05.2026"),
        new("d MMM yyyy", "15 May 2026")
    ];

    private static readonly IReadOnlyCollection<CountryCallingCodeOption> CountryOptions = BuildCountryOptions();

    public static AccountOptionsResponse CreateResponse()
        => new(LanguageOptions, TimeZoneOptions, ThemeOptions, DateFormatOptions, CountryOptions);

    public static IReadOnlyCollection<AccountOptionItem> GetDateFormats() => DateFormatOptions;

    private static IReadOnlyCollection<CountryCallingCodeOption> BuildCountryOptions()
    {
        var phoneUtil = PhoneNumberUtil.GetInstance();
        var regionCodes = phoneUtil.GetSupportedRegions();

        return regionCodes
            .Select(code =>
            {
                var dialCode = phoneUtil.GetCountryCodeForRegion(code);
                string name;
                try
                {
                    name = new RegionInfo(code).EnglishName;
                }
                catch (ArgumentException)
                {
                    return null;
                }

                return new CountryCallingCodeOption(code, name, $"+{dialCode}");
            })
            .Where(x => x is not null)
            .Select(x => x!)
            .OrderBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
