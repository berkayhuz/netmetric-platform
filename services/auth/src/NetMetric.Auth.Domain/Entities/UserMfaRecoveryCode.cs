// <copyright file="UserMfaRecoveryCode.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.Auth.Domain.Entities;

public sealed class UserMfaRecoveryCode : AuditableEntity
{
    public Guid UserId { get; set; }
    public string CodeHash { get; set; } = null!;
    public DateTime? ConsumedAtUtc { get; set; }
    public string? ConsumedByIpAddress { get; set; }
    public User? User { get; set; }
    public bool IsConsumed => ConsumedAtUtc.HasValue;
    public void Consume(DateTime utcNow, string? ipAddress)
    {
        ConsumedAtUtc = utcNow;
        ConsumedByIpAddress = ipAddress;
        UpdatedAt = utcNow;
    }
}
