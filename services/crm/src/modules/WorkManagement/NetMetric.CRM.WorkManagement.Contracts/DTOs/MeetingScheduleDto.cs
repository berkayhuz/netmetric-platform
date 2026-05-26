// <copyright file="MeetingScheduleDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkManagement.Contracts.DTOs;

public sealed record MeetingScheduleDto(Guid Id, string Title, DateTime StartsAtUtc, DateTime EndsAtUtc, string OrganizerEmail, bool RequiresExternalSync);
