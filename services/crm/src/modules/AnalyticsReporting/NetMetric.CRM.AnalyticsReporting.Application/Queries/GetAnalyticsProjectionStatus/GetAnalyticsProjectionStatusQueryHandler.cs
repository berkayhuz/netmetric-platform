// <copyright file="GetAnalyticsProjectionStatusQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetAnalyticsProjectionStatus;

public sealed class GetAnalyticsProjectionStatusQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetAnalyticsProjectionStatusQuery, AnalyticsProjectionStatusDto>
{
    public async Task<AnalyticsProjectionStatusDto> Handle(GetAnalyticsProjectionStatusQuery request, CancellationToken cancellationToken)
    {
        var lastAttempt = await dbContext.ProjectionRuns
            .AsNoTracking()
            .OrderByDescending(x => x.StartedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var lastSuccess = await dbContext.ProjectionRuns
            .AsNoTracking()
            .Where(x => x.Status == AnalyticsProjectionRunStatus.Succeeded)
            .OrderByDescending(x => x.CompletedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var tenantSummaryAt = await dbContext.TenantSnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);
        var salesFunnelAt = await dbContext.SalesFunnelSnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);
        var campaignRoiAt = await dbContext.CampaignRoiSnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);
        var revenueAgingAt = await dbContext.RevenueAgingSnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);
        var supportKpiAt = await dbContext.SupportKpiSnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);
        var userProductivityAt = await dbContext.UserProductivitySnapshots.AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);

        return new AnalyticsProjectionStatusDto(
            lastAttempt?.Status.ToString() ?? "NotStarted",
            lastAttempt?.StartedAtUtc,
            lastSuccess?.CompletedAtUtc,
            lastSuccess?.ProjectedTenantCount ?? 0,
            lastAttempt?.Status == AnalyticsProjectionRunStatus.Failed ? lastAttempt.ErrorMessage : null,
            tenantSummaryAt,
            salesFunnelAt,
            campaignRoiAt,
            revenueAgingAt,
            supportKpiAt,
            userProductivityAt);
    }
}
