// <copyright file="MarketingConsent.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;

public sealed class MarketingConsent : AuditableEntity
{
    private MarketingConsent() { }

    public MarketingConsent(string emailAddress, string emailHash, string status, string source, bool doubleOptInRequired = false)
    {
        EmailAddress = Guard.AgainstNullOrWhiteSpace(emailAddress).Trim().ToLowerInvariant();
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        Status = Guard.AgainstNullOrWhiteSpace(status);
        Source = Guard.AgainstNullOrWhiteSpace(source);
        DoubleOptInRequired = doubleOptInRequired;
        GrantedAtUtc = status == MarketingConsentStatuses.Granted ? DateTime.UtcNow : null;
    }

    public string EmailAddress { get; private set; } = string.Empty;
    public string EmailHash { get; private set; } = string.Empty;
    public string Status { get; private set; } = MarketingConsentStatuses.Unknown;
    public string Source { get; private set; } = string.Empty;
    public bool DoubleOptInRequired { get; private set; }
    public string? DoubleOptInTokenHash { get; private set; }
    public DateTime? GrantedAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }

    public void Grant(string source, bool doubleOptInRequired, string? tokenHash = null)
    {
        Source = Guard.AgainstNullOrWhiteSpace(source);
        DoubleOptInRequired = doubleOptInRequired;
        DoubleOptInTokenHash = string.IsNullOrWhiteSpace(tokenHash) ? null : tokenHash.Trim();
        Status = doubleOptInRequired ? MarketingConsentStatuses.PendingDoubleOptIn : MarketingConsentStatuses.Granted;
        GrantedAtUtc = doubleOptInRequired ? null : DateTime.UtcNow;
        RevokedAtUtc = null;
    }

    public void ConfirmDoubleOptIn()
    {
        Status = MarketingConsentStatuses.Granted;
        GrantedAtUtc = DateTime.UtcNow;
        RevokedAtUtc = null;
    }

    public void Revoke(string source)
    {
        Source = Guard.AgainstNullOrWhiteSpace(source);
        Status = MarketingConsentStatuses.Revoked;
        RevokedAtUtc = DateTime.UtcNow;
    }
}

public static class MarketingConsentStatuses
{
    public const string Unknown = "unknown";
    public const string PendingDoubleOptIn = "pending-double-opt-in";
    public const string Granted = "granted";
    public const string Revoked = "revoked";
}
