// <copyright file="MarketingCampaignScheduler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingCampaignScheduler(
    MarketingAutomationDbContext dbContext,
    IMarketingConsentEnforcementService consentEnforcement,
    IMarketingEmailDeliveryProvider deliveryProvider,
    IOptions<MarketingAutomationOptions> options,
    ILogger<MarketingCampaignScheduler> logger) : IMarketingCampaignScheduler
{
    private readonly MarketingAutomationOptions _options = options.Value;

    public async Task<int> ScheduleDueCampaignsAsync(CancellationToken cancellationToken)
    {
        if (!_options.EngineEnabled)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var campaigns = await dbContext.Campaigns
            .Where(x => x.Status == CampaignStatuses.Scheduled && x.ScheduledAtUtc <= now)
            .OrderBy(x => x.ScheduledAtUtc)
            .Take(Math.Clamp(_options.BatchSize, 1, 200))
            .ToListAsync(cancellationToken);

        var queued = 0;
        foreach (var campaign in campaigns)
        {
            campaign.MarkRunning(now);
            var members = await dbContext.CampaignMembers
                .Where(x => x.TenantId == campaign.TenantId && x.CampaignId == campaign.Id)
                .ToListAsync(cancellationToken);

            foreach (var member in members)
            {
                var key = $"{campaign.TenantId:N}:{campaign.Id:N}:{member.EmailHash}";
                if (await dbContext.MarketingEmailDeliveries.IgnoreQueryFilters().AnyAsync(x => x.TenantId == campaign.TenantId && x.IdempotencyKey == key, cancellationToken))
                {
                    continue;
                }

                var eligibility = await consentEnforcement.CheckAsync(campaign.TenantId, member.EmailAddress, campaign.Id, cancellationToken);
                member.SetEligibility(eligibility.ConsentStatus, eligibility.Suppressed);
                var delivery = new MarketingEmailDelivery(campaign.Id, null, null, member.EmailHash, key, now, $"{campaign.Id:N}-{member.EmailHash}", _options.MaxAttempts)
                {
                    TenantId = campaign.TenantId
                };

                if (!eligibility.Allowed)
                {
                    delivery.MarkSuppressed(eligibility.Reason);
                }

                dbContext.MarketingEmailDeliveries.Add(delivery);
                queued += 1;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return queued;
    }

    public async Task<int> ProcessDueDeliveriesAsync(CancellationToken cancellationToken)
    {
        if (!_options.EngineEnabled || !_options.EmailDeliveryEnabled)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var deliveries = await dbContext.MarketingEmailDeliveries
            .Where(x => (x.Status == MarketingDeliveryStatuses.Queued || x.Status == MarketingDeliveryStatuses.Retrying) && x.NextAttemptAtUtc <= now)
            .OrderBy(x => x.NextAttemptAtUtc)
            .Take(Math.Clamp(_options.BatchSize, 1, 200))
            .ToListAsync(cancellationToken);

        foreach (var delivery in deliveries)
        {
            try
            {
                delivery.MarkAttempt();
                await deliveryProvider.SendAsync(new MarketingEmailDeliveryProviderRequest(delivery.TenantId, delivery.CampaignId, delivery.EmailHash, delivery.CorrelationId), cancellationToken);
                delivery.MarkSent(DateTime.UtcNow);
                var member = await dbContext.CampaignMembers.FirstOrDefaultAsync(x => x.TenantId == delivery.TenantId && x.CampaignId == delivery.CampaignId && x.EmailHash == delivery.EmailHash, cancellationToken);
                member?.MarkSent(DateTime.UtcNow);
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(exception, "Marketing delivery failed. DeliveryId={DeliveryId} CampaignId={CampaignId}", delivery.Id, delivery.CampaignId);
                if (delivery.AttemptNumber < delivery.MaxAttempts)
                {
                    delivery.MarkRetry(DateTime.UtcNow.AddSeconds(_options.BaseRetryDelaySeconds * Math.Max(1, delivery.AttemptNumber)), "delivery_failed", MarketingUtilities.Sanitize(exception.Message));
                }
                else
                {
                    delivery.MarkFailed(DateTime.UtcNow, "delivery_failed", MarketingUtilities.Sanitize(exception.Message));
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return deliveries.Count;
    }
}
