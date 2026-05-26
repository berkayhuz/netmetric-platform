// <copyright file="GetTicketTimelineQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketManagement.Application.Features.Timeline.Queries.GetTicketTimeline;

public sealed class GetTicketTimelineQueryHandler(
    ITicketManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetTicketTimelineQuery, IReadOnlyList<TicketTimelineEventDto>>
{
    public async Task<IReadOnlyList<TicketTimelineEventDto>> Handle(GetTicketTimelineQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.TicketsResource);
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var allowedTicketExists = await dbContext.Tickets.AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.AssignedUserId, x => x.AssignedUserId)
            .AnyAsync(x => x.Id == request.TicketId, cancellationToken);
        if (!allowedTicketExists)
        {
            return [];
        }

        var statusEvents = await dbContext.TicketStatusHistories
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.TicketId == request.TicketId)
            .Select(x => new TicketTimelineEventDto(
                "status-change",
                x.ChangedAt,
                $"Status changed from {x.OldStatus} to {x.NewStatus}",
                canSeeInternalNotes ? x.Note : null))
            .ToListAsync(cancellationToken);

        var priorityEvents = await dbContext.TicketPriorityHistories
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.TicketId == request.TicketId)
            .Select(x => new TicketTimelineEventDto(
                "priority-change",
                x.ChangedAt,
                $"Priority changed from {x.OldPriority} to {x.NewPriority}",
                null))
            .ToListAsync(cancellationToken);

        var commentEvents = await dbContext.TicketComments
            .AsNoTracking()
            .Where(x => x.TenantId == scope.TenantId && x.TicketId == request.TicketId && (canSeeInternalNotes || !x.IsInternal))
            .Select(x => new TicketTimelineEventDto(
                x.IsInternal ? "internal-comment" : "comment",
                x.CreatedAt,
                "Comment added",
                x.Comment))
            .ToListAsync(cancellationToken);

        return statusEvents
            .Concat(priorityEvents)
            .Concat(commentEvents)
            .OrderByDescending(x => x.OccurredAt)
            .ToList();
    }
}
