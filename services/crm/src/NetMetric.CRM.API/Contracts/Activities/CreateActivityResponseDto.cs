// <copyright file="CreateActivityResponseDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Contracts.Activities;

public sealed record CreateActivityResponseDto(
    Guid ActivityId,
    string Type,
    DateTime CreatedAtUtc,
    string SourceEntityType,
    Guid SourceEntityId,
    ActivityTimelineItemDto TimelineItem);
