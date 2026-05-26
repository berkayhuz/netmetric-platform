// <copyright file="AccountOptionsContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Preferences;

public sealed record AccountOptionItem(string Value, string Label);

public sealed record CountryCallingCodeOption(string Iso2, string Name, string DialCode);

public sealed record AccountOptionsResponse(
    IReadOnlyCollection<AccountOptionItem> Languages,
    IReadOnlyCollection<AccountOptionItem> TimeZones,
    IReadOnlyCollection<AccountOptionItem> Themes,
    IReadOnlyCollection<AccountOptionItem> DateFormats,
    IReadOnlyCollection<CountryCallingCodeOption> PhoneCountries);
