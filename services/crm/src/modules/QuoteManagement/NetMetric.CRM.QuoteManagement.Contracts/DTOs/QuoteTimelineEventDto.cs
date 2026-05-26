// <copyright file="QuoteTimelineEventDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record QuoteTimelineEventDto(DateTime OccurredAt, string EventType, string Title, string? Description);
