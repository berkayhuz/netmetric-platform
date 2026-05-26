// <copyright file="CreateActivityRequestDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Contracts.Activities;

public sealed record CreateActivityRequestDto(
    string Type,
    DateTime? OccurredAtUtc,
    string? Title,
    string? Description,
    IReadOnlyList<CreateActivityRelatedRecordDto>? RelatedRecords,
    CreateActivityPayloadDto? Payload);

public sealed record CreateActivityRelatedRecordDto(
    string EntityType,
    Guid EntityId,
    string RelationRole);

public sealed record CreateActivityPayloadDto(
    string? Body,
    string? Direction,
    string? Outcome,
    int? DurationSeconds,
    string? Summary,
    string? Subject,
    string? BodySummary,
    IReadOnlyList<string>? To,
    IReadOnlyList<string>? Cc,
    string? Details,
    Guid? OwnerUserId,
    DateTime? DueAtUtc,
    string? Priority,
    DateTime? StartUtc,
    DateTime? EndUtc,
    string? Location,
    IReadOnlyList<Guid>? AttendeeUserIds);
