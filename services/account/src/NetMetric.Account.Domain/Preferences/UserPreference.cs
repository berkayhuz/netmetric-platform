// <copyright file="UserPreference.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;
using NetMetric.Localization;

namespace NetMetric.Account.Domain.Preferences;

public sealed class UserPreference
{
    private UserPreference()
    {
        Theme = ThemePreference.System;
        Language = "en-US";
        TimeZone = "UTC";
        DateFormat = "yyyy-MM-dd";
        PostLoginDestination = PostLoginDestinationPreference.Account;
        CrmDashboardPreferencesJson = null;
        Version = [];
    }

    private UserPreference(Guid id, TenantId tenantId, UserId userId, DateTimeOffset utcNow)
        : this()
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public ThemePreference Theme { get; private set; }
    public string Language { get; private set; }
    public string TimeZone { get; private set; }
    public string DateFormat { get; private set; }
    public PostLoginDestinationPreference PostLoginDestination { get; private set; }
    public Guid? DefaultOrganizationId { get; private set; }
    public Guid? FaviconMediaAssetId { get; private set; }
    public string? FaviconUrl { get; private set; }
    public string? CrmDashboardPreferencesJson { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] Version { get; private set; }

    public static UserPreference CreateDefault(TenantId tenantId, UserId userId, DateTimeOffset utcNow)
        => new(Guid.NewGuid(), tenantId, userId, utcNow);

    public void Update(
        ThemePreference theme,
        string language,
        string timeZone,
        string dateFormat,
        PostLoginDestinationPreference postLoginDestination,
        Guid? defaultOrganizationId,
        string? crmDashboardPreferencesJson,
        DateTimeOffset utcNow)
    {
        Theme = theme;
        Language = NetMetricCultures.Normalize(language)
            ?? throw new DomainValidationException($"{nameof(language)} must be one of: {string.Join(", ", NetMetricCultures.SupportedCultureNames)}.");
        TimeZone = Normalize(timeZone, 100, nameof(timeZone));
        DateFormat = Normalize(dateFormat, 40, nameof(dateFormat));
        PostLoginDestination = postLoginDestination;
        DefaultOrganizationId = defaultOrganizationId == Guid.Empty ? null : defaultOrganizationId;
        CrmDashboardPreferencesJson = NormalizeOptionalOrNull(
            crmDashboardPreferencesJson,
            200_000,
            nameof(crmDashboardPreferencesJson));
        UpdatedAt = utcNow;
    }

    public void AssignFavicon(Guid mediaAssetId, string publicUrl, DateTimeOffset utcNow)
    {
        FaviconMediaAssetId = mediaAssetId;
        FaviconUrl = NormalizeOptional(publicUrl, 2048, nameof(publicUrl));
        UpdatedAt = utcNow;
    }

    public void ClearFavicon(DateTimeOffset utcNow)
    {
        FaviconMediaAssetId = null;
        FaviconUrl = null;
        UpdatedAt = utcNow;
    }

    private static string Normalize(string value, int maxLength, string name)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            throw new DomainValidationException($"{name} is required.");
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException($"{name} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string NormalizeOptional(string value, int maxLength, string name)
    {
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException($"{name} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }

    private static string? NormalizeOptionalOrNull(string? value, int maxLength, string name)
    {
        if (value is null)
        {
            return null;
        }

        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new DomainValidationException($"{name} cannot exceed {maxLength} characters.");
        }

        return normalized;
    }
}
