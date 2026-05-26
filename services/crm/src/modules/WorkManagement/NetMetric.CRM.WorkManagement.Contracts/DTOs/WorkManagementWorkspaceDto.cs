// <copyright file="WorkManagementWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkManagement.Contracts.DTOs;

public sealed record WorkManagementWorkspaceDto(IReadOnlyList<WorkTaskDto> Tasks, IReadOnlyList<MeetingScheduleDto> Meetings, int OpenTaskCount, int UpcomingMeetingCount);
