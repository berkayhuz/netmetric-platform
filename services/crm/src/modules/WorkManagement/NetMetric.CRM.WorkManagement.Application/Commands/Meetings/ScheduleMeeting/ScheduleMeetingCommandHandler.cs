// <copyright file="ScheduleMeetingCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Meetings.ScheduleMeeting;

public sealed class ScheduleMeetingCommandHandler(IWorkManagementDbContext dbContext) : IRequestHandler<ScheduleMeetingCommand, MeetingScheduleDto>
{
    public async Task<MeetingScheduleDto> Handle(ScheduleMeetingCommand request, CancellationToken cancellationToken)
    {
        var entity = new MeetingSchedule(request.Title, request.StartsAtUtc, request.EndsAtUtc, request.OrganizerEmail, request.AttendeeSummary, request.RequiresExternalSync);
        await dbContext.Meetings.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new MeetingScheduleDto(entity.Id, entity.Title, entity.StartsAtUtc, entity.EndsAtUtc, entity.OrganizerEmail, entity.RequiresExternalSync);
    }
}
