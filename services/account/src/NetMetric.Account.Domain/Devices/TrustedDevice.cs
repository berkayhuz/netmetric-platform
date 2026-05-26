// <copyright file="TrustedDevice.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Devices;

public sealed class TrustedDevice
{
    internal TrustedDevice()
    {
        Name = string.Empty;
        DeviceTokenHash = string.Empty;
        UserAgent = string.Empty;
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public string DeviceTokenHash { get; private set; }
    public string? IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTimeOffset TrustedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public string? RevocationReason { get; private set; }
    public byte[] Version { get; private set; }

    public bool IsActive(DateTimeOffset utcNow) => RevokedAt is null && ExpiresAt > utcNow;

    public void Revoke(string reason, DateTimeOffset utcNow)
    {
        if (RevokedAt is not null)
        {
            return;
        }

        RevokedAt = utcNow;
        RevocationReason = string.IsNullOrWhiteSpace(reason) ? "revoked" : reason.Trim();
    }
}
