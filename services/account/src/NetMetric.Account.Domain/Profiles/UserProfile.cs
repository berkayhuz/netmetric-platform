// <copyright file="UserProfile.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;
using NetMetric.Localization;

namespace NetMetric.Account.Domain.Profiles;

public sealed class UserProfile
{
    private UserProfile()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        DisplayName = string.Empty;
        TimeZone = "UTC";
        Culture = "en-US";
        Version = [];
    }

    private UserProfile(Guid id, TenantId tenantId, UserId userId, string firstName, string lastName, string culture, DateTimeOffset utcNow)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        FirstName = NormalizeRequired(firstName, 100, nameof(firstName));
        LastName = NormalizeRequired(lastName, 100, nameof(lastName));
        DisplayName = BuildDisplayName(FirstName, LastName);
        TimeZone = "UTC";
        Culture = NetMetricCultures.Normalize(culture)
            ?? throw new DomainValidationException($"{nameof(culture)} must be one of: {string.Join(", ", NetMetricCultures.SupportedCultureNames)}.");
        CreatedAt = utcNow;
        UpdatedAt = utcNow;
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string DisplayName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? AvatarUrl { get; private set; }
    public Guid? AvatarMediaAssetId { get; private set; }
    public string? JobTitle { get; private set; }
    public string? Department { get; private set; }
    public string TimeZone { get; private set; }
    public string Culture { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public byte[] Version { get; private set; }

    public static UserProfile Create(TenantId tenantId, UserId userId, string firstName, string lastName, DateTimeOffset utcNow, string culture = NetMetricCultures.DefaultCulture)
        => new(Guid.NewGuid(), tenantId, userId, firstName, lastName, culture, utcNow);

    public void Update(
        string firstName,
        string lastName,
        string? phoneNumber,
        string? jobTitle,
        string? department,
        string timeZone,
        string culture,
        DateTimeOffset utcNow)
    {
        FirstName = NormalizeRequired(firstName, 100, nameof(firstName));
        LastName = NormalizeRequired(lastName, 100, nameof(lastName));
        DisplayName = BuildDisplayName(FirstName, LastName);
        PhoneNumber = NormalizeOptional(phoneNumber, 32, nameof(phoneNumber));
        JobTitle = NormalizeOptional(jobTitle, 120, nameof(jobTitle));
        Department = NormalizeOptional(department, 120, nameof(department));
        TimeZone = NormalizeRequired(timeZone, 100, nameof(timeZone));
        Culture = NetMetricCultures.Normalize(culture)
            ?? throw new DomainValidationException($"{nameof(culture)} must be one of: {string.Join(", ", NetMetricCultures.SupportedCultureNames)}.");
        UpdatedAt = utcNow;
    }

    public void AssignManagedAvatar(Guid mediaAssetId, string publicUrl, DateTimeOffset utcNow)
    {
        AvatarMediaAssetId = mediaAssetId;
        AvatarUrl = NormalizeRequired(publicUrl, 2048, nameof(publicUrl));
        UpdatedAt = utcNow;
    }

    public void ClearAvatar(DateTimeOffset utcNow)
    {
        AvatarMediaAssetId = null;
        AvatarUrl = null;
        UpdatedAt = utcNow;
    }

    private static string BuildDisplayName(string firstName, string lastName) => $"{firstName} {lastName}".Trim();

    private static string NormalizeRequired(string value, int maxLength, string name)
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

    private static string? NormalizeOptional(string? value, int maxLength, string name)
    {
        var normalized = value?.Trim();
        if (string.IsNullOrWhiteSpace(normalized))
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
