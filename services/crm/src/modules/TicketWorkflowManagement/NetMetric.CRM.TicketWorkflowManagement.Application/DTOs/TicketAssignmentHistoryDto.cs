// <copyright file="TicketAssignmentHistoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

public sealed class TicketAssignmentHistoryDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public Guid? PreviousOwnerUserId { get; init; }
    public Guid? NewOwnerUserId { get; init; }
    public Guid? PreviousQueueId { get; init; }
    public Guid? NewQueueId { get; init; }
    public Guid? ChangedByUserId { get; init; }
    public string? Reason { get; init; }
    public DateTime ChangedAtUtc { get; init; }
}
