// <copyright file="GetSupportKpisQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetSupportKpis;

public sealed class GetSupportKpisQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetSupportKpisQuery, IReadOnlyCollection<SupportKpiDto>>
{
    public async Task<IReadOnlyCollection<SupportKpiDto>> Handle(GetSupportKpisQuery request, CancellationToken cancellationToken)
    {
        var snapshot = await dbContext.SupportKpiSnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .OrderByDescending(x => x.SnapshotAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return snapshot is null
            ? []
            : [new SupportKpiDto(snapshot.OpenTickets, snapshot.OverdueTickets, snapshot.FirstResponseHours, snapshot.ResolutionHours)];
    }
}
