// <copyright file="SuppressionEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;

public sealed class SuppressionEntry : AuditableEntity
{
    private SuppressionEntry() { }

    public SuppressionEntry(string emailAddress, string emailHash, string reason, string source)
    {
        EmailAddress = Guard.AgainstNullOrWhiteSpace(emailAddress).Trim().ToLowerInvariant();
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        Reason = Guard.AgainstNullOrWhiteSpace(reason);
        Source = Guard.AgainstNullOrWhiteSpace(source);
        SuppressedAtUtc = DateTime.UtcNow;
    }

    public string EmailAddress { get; private set; } = string.Empty;
    public string EmailHash { get; private set; } = string.Empty;
    public string Reason { get; private set; } = string.Empty;
    public string Source { get; private set; } = string.Empty;
    public DateTime SuppressedAtUtc { get; private set; }

    public void UpdateReason(string reason, string source)
    {
        Reason = Guard.AgainstNullOrWhiteSpace(reason);
        Source = Guard.AgainstNullOrWhiteSpace(source);
        SuppressedAtUtc = DateTime.UtcNow;
    }
}
