// <copyright file="MarketingConsentEnforcementService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingConsentEnforcementService(MarketingAutomationDbContext dbContext) : IMarketingConsentEnforcementService
{
    public async Task<MarketingEligibilityResult> CheckAsync(Guid tenantId, string emailAddress, Guid? campaignId, CancellationToken cancellationToken)
    {
        var emailHash = MarketingUtilities.HashEmail(emailAddress);
        var consent = await dbContext.MarketingConsents
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.EmailHash == emailHash, cancellationToken);
        var suppressed = await dbContext.SuppressionEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(x => x.TenantId == tenantId && x.EmailHash == emailHash, cancellationToken);

        if (suppressed)
        {
            return new MarketingEligibilityResult(false, "recipient suppressed", consent?.Status ?? MarketingConsentStatuses.Unknown, true);
        }

        if (consent?.Status != MarketingConsentStatuses.Granted)
        {
            return new MarketingEligibilityResult(false, "marketing consent not granted", consent?.Status ?? MarketingConsentStatuses.Unknown, false);
        }

        if (campaignId.HasValue)
        {
            var campaign = await dbContext.Campaigns
                .IgnoreQueryFilters()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == campaignId.Value, cancellationToken);
            if (campaign is not null)
            {
                var since = DateTime.UtcNow.AddDays(-campaign.FrequencyCapWindowDays);
                var sentCount = await dbContext.MarketingEmailDeliveries
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .CountAsync(x => x.TenantId == tenantId &&
                                     x.EmailHash == emailHash &&
                                     x.Status == MarketingDeliveryStatuses.Sent &&
                                     x.SentAtUtc >= since,
                        cancellationToken);
                if (sentCount >= campaign.FrequencyCapPerContact)
                {
                    return new MarketingEligibilityResult(false, "frequency cap reached", consent.Status, false);
                }
            }
        }

        return new MarketingEligibilityResult(true, "eligible", consent.Status, false);
    }
}
