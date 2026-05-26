// <copyright file="WorkTaskDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkManagement.Contracts.DTOs;

public sealed record WorkTaskDto(
    Guid Id,
    string Title,
    string Description,
    Guid? OwnerUserId,
    DateTime DueAtUtc,
    DateTime? ReminderAtUtc,
    int Priority,
    string Status,
    DateTime? CompletedAtUtc,
    Guid? CompletedByUserId,
    string? CompletionNote);
