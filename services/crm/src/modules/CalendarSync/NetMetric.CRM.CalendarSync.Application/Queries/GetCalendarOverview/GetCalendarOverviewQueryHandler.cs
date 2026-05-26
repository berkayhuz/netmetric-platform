// <copyright file="GetCalendarOverviewQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;

namespace NetMetric.CRM.CalendarSync.Application.Queries.GetCalendarOverview;

public sealed class GetCalendarOverviewQueryHandler(ICalendarSyncDbContext dbContext) : IRequestHandler<GetCalendarOverviewQuery, CalendarOverviewDto>
{
    public async Task<CalendarOverviewDto> Handle(GetCalendarOverviewQuery request, CancellationToken cancellationToken)
    {
        var connectionRows = await dbContext.Connections
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var connections = connectionRows
            .Select(x => new CalendarConnectionDto(x.Id, x.Name, x.Provider.ToString(), x.CalendarIdentifier, x.SyncDirection.ToString(), x.IsActive))
            .ToList();

        var runRows = await dbContext.SyncRuns
            .OrderByDescending(x => x.CreatedAt)
            .Take(20)
            .ToListAsync(cancellationToken);

        var recentRuns = runRows
            .Select(x => new CalendarSyncRunDto(x.Id, x.ConnectionId, x.Status.ToString(), x.ImportedCount, x.ExportedCount, x.ErrorMessage))
            .ToList();

        return new CalendarOverviewDto(connections, recentRuns, connections.Count(x => x.IsActive));
    }
}
