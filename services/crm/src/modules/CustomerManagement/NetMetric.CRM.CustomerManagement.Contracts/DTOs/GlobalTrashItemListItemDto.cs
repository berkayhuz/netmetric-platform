// <copyright file="GlobalTrashItemListItemDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Contracts.DTOs;

public sealed record GlobalTrashItemListItemDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string DisplayName,
    string? Summary,
    string SourceModule,
    string? OriginalRoute,
    DateTime DeletedAtUtc,
    Guid? DeletedByUserId,
    string? DeletedByDisplayName,
    DateTime ExpiresAtUtc,
    string Status);
