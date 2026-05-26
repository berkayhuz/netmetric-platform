// <copyright file="GetUserProductivityQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetUserProductivity;

public sealed class GetUserProductivityQueryHandler(IAnalyticsReportingDbContext dbContext)
    : IRequestHandler<GetUserProductivityQuery, IReadOnlyCollection<ProductivityDto>>
{
    public async Task<IReadOnlyCollection<ProductivityDto>> Handle(GetUserProductivityQuery request, CancellationToken cancellationToken)
    {
        var latestSnapshotAtUtc = await dbContext.UserProductivitySnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId)
            .MaxAsync(x => (DateTime?)x.SnapshotAtUtc, cancellationToken);

        if (latestSnapshotAtUtc is null)
        {
            return [];
        }

        return await dbContext.UserProductivitySnapshots
            .AsNoTracking()
            .Where(x => x.TenantId == request.TenantId && x.SnapshotAtUtc == latestSnapshotAtUtc)
            .OrderByDescending(x => x.DealsWon)
            .ThenByDescending(x => x.TicketsClosed)
            .ThenByDescending(x => x.ActivitiesCompleted)
            .Select(x => new ProductivityDto(x.UserId, x.UserName, x.ActivitiesCompleted, x.TicketsClosed, x.DealsWon))
            .ToListAsync(cancellationToken);
    }
}
