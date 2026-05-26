// <copyright file="UserSession.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Sessions;

public sealed class UserSession
{
    internal UserSession()
    {
        RefreshTokenHash = string.Empty;
        UserAgent = string.Empty;
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public string RefreshTokenHash { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public string? ApproximateLocation { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset LastSeenAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    public byte[] Version { get; private set; }

    public bool IsActive(DateTimeOffset utcNow) => RevokedAt is null && ExpiresAt > utcNow;

    public static UserSession Create(
        Guid id,
        TenantId tenantId,
        UserId userId,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        string? ipAddress,
        string? userAgent,
        string? deviceName = null,
        string? approximateLocation = null)
    {
        return new UserSession
        {
            Id = id,
            TenantId = tenantId,
            UserId = userId,
            RefreshTokenHash = $"external:{id:D}",
            DeviceName = NormalizeOptional(deviceName, 160),
            IpAddress = NormalizeOptional(ipAddress, 64),
            UserAgent = NormalizeRequired(userAgent ?? "unknown", 1024),
            ApproximateLocation = NormalizeOptional(approximateLocation, 160),
            CreatedAt = createdAt,
            LastSeenAt = createdAt,
            ExpiresAt = expiresAt,
            RevokedAt = null,
            RevocationReason = null,
            Version = []
        };
    }

    public void Touch(DateTimeOffset utcNow, DateTimeOffset expiresAt, string? ipAddress, string? userAgent)
    {
        LastSeenAt = utcNow;
        ExpiresAt = expiresAt > ExpiresAt ? expiresAt : ExpiresAt;
        IpAddress = NormalizeOptional(ipAddress, 64);
        UserAgent = NormalizeRequired(userAgent ?? UserAgent, 1024);
    }

    public void Revoke(string reason, DateTimeOffset utcNow)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = utcNow;
        RevocationReason = string.IsNullOrWhiteSpace(reason) ? "revoked" : reason.Trim();
    }

    private static string NormalizeRequired(string value, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
        {
            return "unknown";
        }

        return normalized.Length > maxLength
            ? normalized[..maxLength]
            : normalized;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();
        return normalized.Length > maxLength
            ? normalized[..maxLength]
            : normalized;
    }
}
