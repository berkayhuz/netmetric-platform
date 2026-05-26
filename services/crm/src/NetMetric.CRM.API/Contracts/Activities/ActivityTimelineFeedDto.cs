// <copyright file="ActivityTimelineFeedDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Contracts.Activities;

public sealed record ActivityTimelineFeedDto(
    IReadOnlyList<ActivityTimelineItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
