// <copyright file="ProductBundleDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.DTOs;

public sealed record ProductBundleDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    string? Segment,
    string? Industry,
    decimal DiscountRate,
    decimal? MinimumBudget,
    IReadOnlyList<ProductBundleItemDto> Items,
    bool IsActive,
    string RowVersion);
