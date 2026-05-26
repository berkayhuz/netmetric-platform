// <copyright file="LeadTimelineEventDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Contracts.DTOs;

public sealed record LeadTimelineEventDto(
    DateTime OccurredAt,
    string EventType,
    string Title,
    string? Description);
