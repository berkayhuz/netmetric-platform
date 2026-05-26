// <copyright file="CampaignRoiProjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;

public sealed class CampaignRoiProjection : AuditableEntity
{
    private CampaignRoiProjection() { }

    public CampaignRoiProjection(Guid campaignId)
    {
        CampaignId = Guard.AgainstEmpty(campaignId);
    }

    public Guid CampaignId { get; private set; }
    public int SentCount { get; private set; }
    public int SuppressedCount { get; private set; }
    public decimal RevenueAmount { get; private set; }
    public decimal CostAmount { get; private set; }
    public decimal RoiPercent { get; private set; }
    public DateTime CalculatedAtUtc { get; private set; }

    public void Recalculate(int sentCount, int suppressedCount, decimal revenueAmount, decimal costAmount)
    {
        SentCount = Math.Max(0, sentCount);
        SuppressedCount = Math.Max(0, suppressedCount);
        RevenueAmount = Math.Max(0, revenueAmount);
        CostAmount = Math.Max(0, costAmount);
        RoiPercent = CostAmount == 0 ? 0 : Math.Round(((RevenueAmount - CostAmount) / CostAmount) * 100, 2);
        CalculatedAtUtc = DateTime.UtcNow;
    }
}
