// <copyright file="UserConsent.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Consents;

public sealed class UserConsent
{
    private UserConsent()
    {
        ConsentType = string.Empty;
        Version = string.Empty;
        Status = ConsentStatus.Accepted;
        VersionToken = [];
    }

    private UserConsent(Guid id, TenantId tenantId, UserId userId, string consentType, string version, ConsentStatus status, DateTimeOffset decidedAt)
        : this()
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        ConsentType = Normalize(consentType, 80, nameof(consentType));
        Version = Normalize(version, 40, nameof(version));
        Status = status;
        DecidedAt = decidedAt;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public string ConsentType { get; private set; }
    public string Version { get; private set; }
    public ConsentStatus Status { get; private set; }
    public DateTimeOffset DecidedAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public byte[] VersionToken { get; private set; }

    public static UserConsent Accept(TenantId tenantId, UserId userId, string consentType, string version, DateTimeOffset utcNow, string? ipAddress, string? userAgent)
    {
        var consent = new UserConsent(Guid.NewGuid(), tenantId, userId, consentType, version, ConsentStatus.Accepted, utcNow);
        consent.IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        consent.UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        return consent;
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
}
