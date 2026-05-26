// <copyright file="TicketStatusHistoryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

public sealed class TicketStatusHistoryDto
{
    public Guid Id { get; init; }
    public Guid TicketId { get; init; }
    public string PreviousStatus { get; init; } = null!;
    public string NewStatus { get; init; } = null!;
    public Guid? ChangedByUserId { get; init; }
    public string? Note { get; init; }
    public DateTime ChangedAtUtc { get; init; }
}
