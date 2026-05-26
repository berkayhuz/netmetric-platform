// <copyright file="OpportunityTimelineEventDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

public sealed record OpportunityTimelineEventDto(DateTime OccurredAt, string EventType, string Title, string? Description);
