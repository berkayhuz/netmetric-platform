// <copyright file="ScheduleMeetingCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkManagement.Contracts.DTOs;

namespace NetMetric.CRM.WorkManagement.Application.Commands.Meetings.ScheduleMeeting;

public sealed record ScheduleMeetingCommand(string Title, DateTime StartsAtUtc, DateTime EndsAtUtc, string OrganizerEmail, string AttendeeSummary, bool RequiresExternalSync) : IRequest<MeetingScheduleDto>;
