// <copyright file="GetEntityTimelineQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerManagement.Application.DTOs.Timeline;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Timeline.Queries.GetEntityTimeline;

public sealed class GetEntityTimelineQuery : IRequest<IReadOnlyList<TimelineEventDto>>
{
    public required string EntityName { get; init; }
    public required Guid EntityId { get; init; }
    public int Take { get; init; } = 50;
}
