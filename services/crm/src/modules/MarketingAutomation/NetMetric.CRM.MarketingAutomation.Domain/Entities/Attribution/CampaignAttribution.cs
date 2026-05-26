// <copyright file="CampaignAttribution.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;

public sealed class CampaignAttribution : AuditableEntity
{
    private CampaignAttribution() { }

    public CampaignAttribution(Guid campaignId, string emailHash, string touchType, decimal revenueAmount, decimal costAmount, string utmSource, string utmMedium, string utmCampaign)
    {
        CampaignId = Guard.AgainstEmpty(campaignId);
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        TouchType = Guard.AgainstNullOrWhiteSpace(touchType);
        RevenueAmount = Math.Max(0, revenueAmount);
        CostAmount = Math.Max(0, costAmount);
        UtmSource = Guard.AgainstNullOrWhiteSpace(utmSource);
        UtmMedium = Guard.AgainstNullOrWhiteSpace(utmMedium);
        UtmCampaign = Guard.AgainstNullOrWhiteSpace(utmCampaign);
        OccurredAtUtc = DateTime.UtcNow;
    }

    public Guid CampaignId { get; private set; }
    public string EmailHash { get; private set; } = string.Empty;
    public string TouchType { get; private set; } = string.Empty;
    public decimal RevenueAmount { get; private set; }
    public decimal CostAmount { get; private set; }
    public string UtmSource { get; private set; } = string.Empty;
    public string UtmMedium { get; private set; } = string.Empty;
    public string UtmCampaign { get; private set; } = string.Empty;
    public DateTime OccurredAtUtc { get; private set; }
}
