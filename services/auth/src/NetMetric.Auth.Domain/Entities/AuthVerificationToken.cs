// <copyright file="AuthVerificationToken.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.Auth.Domain.Entities;

public sealed class AuthVerificationToken : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Purpose { get; set; } = null!;
    public string TokenHash { get; set; } = null!;
    public string? Target { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? ConsumedAtUtc { get; set; }
    public string? ConsumedByIpAddress { get; set; }
    public int Attempts { get; set; }
    public bool IsConsumed => ConsumedAtUtc.HasValue;
    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;
    public void Consume(DateTime utcNow, string? ipAddress)
    {
        ConsumedAtUtc = utcNow;
        ConsumedByIpAddress = ipAddress;
    }
}
