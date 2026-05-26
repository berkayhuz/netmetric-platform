// <copyright file="TicketQueueDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

public sealed class TicketQueueDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public string AssignmentStrategy { get; init; } = null!;
    public bool IsDefault { get; init; }
}
