// <copyright file="Customer360ActivityDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed record Customer360ActivityDto(
    Guid Id,
    string SubjectType,
    Guid SubjectId,
    string Name,
    string Category,
    string? Channel,
    string? EntityType,
    Guid? RelatedEntityId,
    string? DataJson,
    DateTime OccurredAtUtc);
