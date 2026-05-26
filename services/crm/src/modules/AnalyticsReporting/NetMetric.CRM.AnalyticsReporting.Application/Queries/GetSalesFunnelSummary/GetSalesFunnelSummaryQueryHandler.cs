// <copyright file="GetSalesFunnelSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetSalesFunnelSummary;

public sealed class GetSalesFunnelSummaryQueryHandler(IAnalyticsReportingDbContext dbContext) : IRequestHandler<GetSalesFunnelSummaryQuery, SalesFunnelSummaryDto>
{
    public async Task<SalesFunnelSummaryDto> Handle(GetSalesFunnelSummaryQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.SalesFunnelSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .OrderByDescending(x => x.SnapshotAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot is null
            ? new SalesFunnelSummaryDto(0, 0, 0, 0, 0)
            : new SalesFunnelSummaryDto(
                snapshot.OpenLeads,
                snapshot.QualifiedLeads,
                snapshot.OpenOpportunities,
                snapshot.WonDeals,
                snapshot.PipelineValue);
    }
}
