// <copyright file="TicketDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.Contracts.DTOs;

public sealed record TicketDetailDto(
    Guid Id,
    string TicketNumber,
    string Subject,
    string? Description,
    TicketStatusType Status,
    PriorityType Priority,
    TicketType TicketType,
    TicketChannelType Channel,
    Guid? AssignedUserId,
    Guid? CustomerId,
    Guid? ContactId,
    Guid? TicketCategoryId,
    Guid? SlaPolicyId,
    DateTime OpenedAt,
    DateTime? ClosedAt,
    DateTime? FirstResponseDueAt,
    DateTime? ResolveDueAt,
    string? Notes,
    bool IsActive,
    byte[] RowVersion,
    IReadOnlyList<TicketCommentDto> Comments);
