// <copyright file="CampaignMember.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.CampaignMembers;

public class CampaignMember : AuditableEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CampaignId { get; private set; }
    public Guid? ContactId { get; private set; }
    public string EmailAddress { get; private set; } = string.Empty;
    public string EmailHash { get; private set; } = string.Empty;
    public string ConsentStatus { get; private set; } = MarketingConsentStatuses.Unknown;
    public bool Suppressed { get; private set; }
    public int SentCount { get; private set; }
    public int OpenCount { get; private set; }
    public int ClickCount { get; private set; }
    public DateTime? LastSentAtUtc { get; private set; }

    private CampaignMember() { }

    public CampaignMember(string code, string name, string? description = null)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public CampaignMember(Guid campaignId, string emailAddress, string emailHash, Guid? contactId = null, string? name = null)
    {
        CampaignId = Guard.AgainstEmpty(campaignId);
        EmailAddress = Guard.AgainstNullOrWhiteSpace(emailAddress).Trim().ToLowerInvariant();
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        ContactId = contactId;
        Code = $"{campaignId:N}:{EmailHash}";
        Name = string.IsNullOrWhiteSpace(name) ? EmailAddress : name.Trim();
    }

    public void Update(string name, string? description)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void SetEligibility(string consentStatus, bool suppressed)
    {
        ConsentStatus = Guard.AgainstNullOrWhiteSpace(consentStatus);
        Suppressed = suppressed;
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        SentCount += 1;
        LastSentAtUtc = sentAtUtc;
    }
}
