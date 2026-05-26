// <copyright file="GetTenantReportSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetTenantReportSummary;

public sealed class GetTenantReportSummaryQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetTenantReportSummaryQuery, IReadOnlyCollection<TenantReportSummaryDto>>
{
    public async Task<IReadOnlyCollection<TenantReportSummaryDto>> Handle(GetTenantReportSummaryQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.TenantSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .OrderByDescending(x => x.SnapshotAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot is null
            ? []
            : [
                new TenantReportSummaryDto(
                    snapshot.TenantId,
                    snapshot.TenantName,
                    snapshot.ActiveUsers,
                    snapshot.Customers,
                    snapshot.Revenue,
                    snapshot.OpenTickets)
            ];
    }
}
