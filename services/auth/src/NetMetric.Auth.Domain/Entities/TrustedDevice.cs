// <copyright file="TrustedDevice.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.Auth.Domain.Entities;

public sealed class TrustedDevice : AuditableEntity
{
    public Guid UserId { get; set; }
    public string DeviceFingerprintHash { get; set; } = null!;
    public string? DeviceName { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime TrustedAtUtc { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
    public string? RevokedReason { get; set; }
    public User? User { get; set; }
    public bool IsRevoked => RevokedAtUtc.HasValue;
    public void MarkSeen(DateTime utcNow, string? ipAddress, string? userAgent)
    {
        LastSeenAtUtc = utcNow;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        UpdatedAt = utcNow;
    }
    public void Revoke(DateTime utcNow, string reason)
    {
        RevokedAtUtc ??= utcNow;
        RevokedReason ??= reason;
        UpdatedAt = utcNow;
    }
}
