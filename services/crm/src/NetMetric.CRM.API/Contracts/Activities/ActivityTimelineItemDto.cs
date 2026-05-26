// <copyright file="ActivityTimelineItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Contracts.Activities;

public sealed record ActivityTimelineItemDto(
    string Id,
    DateTime OccurredAtUtc,
    string Type,
    string Title,
    string? Description,
    string? Status,
    string SourceModule,
    string SourceEntityType,
    Guid? SourceEntityId,
    Guid? ActorUserId,
    Guid? OwnerUserId,
    IReadOnlyList<ActivityRelatedRecordDto> RelatedRecords,
    IReadOnlyDictionary<string, string?> Metadata);
