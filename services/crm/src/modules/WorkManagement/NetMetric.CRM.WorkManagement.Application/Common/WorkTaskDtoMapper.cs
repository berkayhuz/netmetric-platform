// <copyright file="WorkTaskDtoMapper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkManagement.Contracts.DTOs;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Application.Common;

internal static class WorkTaskDtoMapper
{
    public static WorkTaskDto ToDto(this WorkTask task)
        => new(
            task.Id,
            task.Title,
            task.Description,
            task.OwnerUserId,
            task.DueAtUtc,
            task.ReminderAtUtc,
            task.Priority,
            task.Status.ToString(),
            task.CompletedAtUtc,
            task.CompletedByUserId,
            task.CompletionNote);
}
