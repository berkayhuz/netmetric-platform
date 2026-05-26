// <copyright file="ActivityRelatedRecordDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Contracts.Activities;

public sealed record ActivityRelatedRecordDto(
    string EntityType,
    Guid EntityId,
    string? DisplayName,
    string? RelationRole);
