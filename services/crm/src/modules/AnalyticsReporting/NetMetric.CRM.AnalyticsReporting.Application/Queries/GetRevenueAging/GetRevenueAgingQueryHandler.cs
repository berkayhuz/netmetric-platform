// <copyright file="GetRevenueAgingQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRevenueAging;

public sealed class GetRevenueAgingQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetRevenueAgingQuery, IReadOnlyCollection<RevenueAgingDto>>
{
    public async Task<IReadOnlyCollection<RevenueAgingDto>> Handle(GetRevenueAgingQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.RevenueAgingSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .OrderByDescending(x => x.SnapshotAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot is null
            ? []
            : [new RevenueAgingDto(snapshot.CurrentAmount, snapshot.Days30, snapshot.Days60, snapshot.Days90Plus)];
    }
}
