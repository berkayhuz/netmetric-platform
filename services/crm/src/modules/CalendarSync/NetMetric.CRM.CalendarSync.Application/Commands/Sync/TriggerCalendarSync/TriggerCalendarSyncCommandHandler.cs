// <copyright file="TriggerCalendarSyncCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;
using NetMetric.CRM.CalendarSync.Contracts.DTOs;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CalendarSync.Application.Commands.Sync.TriggerCalendarSync;

public sealed class TriggerCalendarSyncCommandHandler(ICalendarSyncDbContext dbContext) : IRequestHandler<TriggerCalendarSyncCommand, CalendarSyncRunDto>
{
    public async Task<CalendarSyncRunDto> Handle(TriggerCalendarSyncCommand request, CancellationToken cancellationToken)
    {
        var connectionExists = await dbContext.Connections.AnyAsync(x => x.Id == request.ConnectionId, cancellationToken);
        if (!connectionExists)
        {
            throw new InvalidOperationException($"Calendar connection '{request.ConnectionId}' was not found.");
        }

        throw new ForbiddenAppException("Calendar sync is disabled until a production calendar provider adapter and worker are configured.");
    }
}
