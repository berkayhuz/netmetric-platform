// <copyright file="AnalyticsProjectionService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.Results;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

public sealed class AnalyticsProjectionService(
    IAnalyticsReportingDbContext dbContext,
    IAnalyticsProjectionSource source,
    ILogger<AnalyticsProjectionService> logger) : IAnalyticsProjectionService
{
    public async Task<AnalyticsProjectionResult> RunOnceAsync(CancellationToken cancellationToken)
    {
        var startedAtUtc = DateTime.UtcNow;
        var correlationId = Guid.NewGuid().ToString("N");
        var run = new AnalyticsProjectionRun(correlationId, startedAtUtc);

        await dbContext.ProjectionRuns.AddAsync(run, cancellationToken);

        try
        {
            var batch = await source.ReadAsync(cancellationToken);
            var projectedAtUtc = batch.ProjectedAtUtc;

            await dbContext.TenantSnapshots.AddRangeAsync(
                batch.Tenants.Select(x => new AnalyticsTenantSnapshot(
                    x.TenantId,
                    x.TenantName,
                    x.ActiveUsers,
                    x.Customers,
                    x.Revenue,
                    x.OpenTickets,
                    projectedAtUtc)),
                cancellationToken);

            await dbContext.SalesFunnelSnapshots.AddRangeAsync(
                batch.SalesFunnels.Select(x => new AnalyticsSalesFunnelSnapshot(
                    x.TenantId,
                    x.OpenLeads,
                    x.QualifiedLeads,
                    x.OpenOpportunities,
                    x.WonDeals,
                    x.PipelineValue,
                    projectedAtUtc)),
                cancellationToken);

            await dbContext.CampaignRoiSnapshots.AddRangeAsync(
                batch.CampaignRoi.Select(x => new AnalyticsCampaignRoiSnapshot(
                    x.TenantId,
                    x.CampaignName,
                    x.Spend,
                    x.Revenue,
                    projectedAtUtc)),
                cancellationToken);

            await dbContext.RevenueAgingSnapshots.AddRangeAsync(
                batch.RevenueAging.Select(x => new AnalyticsRevenueAgingSnapshot(
                    x.TenantId,
                    x.CurrentAmount,
                    x.Days30,
                    x.Days60,
                    x.Days90Plus,
                    projectedAtUtc)),
                cancellationToken);

            await dbContext.SupportKpiSnapshots.AddRangeAsync(
                batch.SupportKpis.Select(x => new AnalyticsSupportKpiSnapshot(
                    x.TenantId,
                    x.OpenTickets,
                    x.OverdueTickets,
                    x.FirstResponseHours,
                    x.ResolutionHours,
                    projectedAtUtc)),
                cancellationToken);

            await dbContext.UserProductivitySnapshots.AddRangeAsync(
                batch.UserProductivity.Select(x => new AnalyticsUserProductivitySnapshot(
                    x.TenantId,
                    x.UserId,
                    x.UserName,
                    x.ActivitiesCompleted,
                    x.TicketsClosed,
                    x.DealsWon,
                    projectedAtUtc)),
                cancellationToken);

            run.MarkSucceeded(batch.Tenants.Select(x => x.TenantId).Distinct().Count(), DateTime.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);

            AnalyticsProjectionMetrics.RecordSuccess(DateTime.UtcNow - startedAtUtc, run.ProjectedTenantCount);
            logger.LogInformation(
                "Analytics projection completed. CorrelationId={CorrelationId} ProjectedTenantCount={ProjectedTenantCount}",
                correlationId,
                run.ProjectedTenantCount);

            return new AnalyticsProjectionResult(correlationId, true, run.ProjectedTenantCount, null);
        }
        catch (Exception ex)
        {
            run.MarkFailed(ex.Message, DateTime.UtcNow);
            await dbContext.SaveChangesAsync(CancellationToken.None);

            AnalyticsProjectionMetrics.RecordFailure(DateTime.UtcNow - startedAtUtc);
            logger.LogError(ex, "Analytics projection failed. CorrelationId={CorrelationId}", correlationId);

            return new AnalyticsProjectionResult(correlationId, false, 0, ex.Message);
        }
    }
}
