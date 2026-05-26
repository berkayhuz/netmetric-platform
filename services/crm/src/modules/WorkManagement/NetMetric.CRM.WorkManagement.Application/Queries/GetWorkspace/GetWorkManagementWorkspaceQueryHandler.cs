// <copyright file="GetWorkManagementWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.CRM.WorkManagement.Domain.Enums;

namespace NetMetric.CRM.WorkManagement.Application.Queries.GetWorkspace;

public sealed class GetWorkManagementWorkspaceQueryHandler(IWorkManagementDbContext dbContext) : IRequestHandler<GetWorkManagementWorkspaceQuery, WorkManagementWorkspaceDto>
{
    public async Task<WorkManagementWorkspaceDto> Handle(GetWorkManagementWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var taskRows = await dbContext.Tasks
            .OrderBy(x => x.DueAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var tasks = taskRows
            .Select(x => new WorkTaskDto(
                x.Id,
                x.Title,
                x.Description,
                x.OwnerUserId,
                x.DueAtUtc,
                x.ReminderAtUtc,
                x.Priority,
                x.Status.ToString(),
                x.CompletedAtUtc,
                x.CompletedByUserId,
                x.CompletionNote))
            .ToList();

        var meetingRows = await dbContext.Meetings
            .Where(x => x.StartsAtUtc >= now)
            .OrderBy(x => x.StartsAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var meetings = meetingRows
            .Select(x => new MeetingScheduleDto(x.Id, x.Title, x.StartsAtUtc, x.EndsAtUtc, x.OrganizerEmail, x.RequiresExternalSync))
            .ToList();

        var openTaskCount = await dbContext.Tasks.CountAsync(x => x.Status != WorkItemStatus.Completed && x.Status != WorkItemStatus.Cancelled, cancellationToken);

        return new WorkManagementWorkspaceDto(tasks, meetings, openTaskCount, meetings.Count);
    }
}
