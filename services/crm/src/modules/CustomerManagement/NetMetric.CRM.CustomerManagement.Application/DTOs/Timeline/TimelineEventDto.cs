// <copyright file="TimelineEventDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Application.DTOs.Timeline;

public sealed class TimelineEventDto
{
    public required string EventType { get; init; }
    public required string Title { get; init; }
    public string? Description { get; init; }
    public DateTime OccurredAt { get; init; }
    public string? Actor { get; init; }
    public Guid? ReferenceId { get; init; }
}
