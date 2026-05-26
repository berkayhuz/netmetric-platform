// <copyright file="PreferenceContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Preferences;

public sealed record UserPreferenceResponse(
    Guid Id,
    string Theme,
    string Language,
    string TimeZone,
    string DateFormat,
    string PostLoginDestination,
    Guid? DefaultOrganizationId,
    string? FaviconUrl,
    string? CrmDashboardPreferencesJson,
    string Version);

public sealed record UpdateUserPreferenceRequest(
    string Theme,
    string Language,
    string TimeZone,
    string DateFormat,
    string PostLoginDestination,
    Guid? DefaultOrganizationId,
    string? CrmDashboardPreferencesJson,
    string? Version);
