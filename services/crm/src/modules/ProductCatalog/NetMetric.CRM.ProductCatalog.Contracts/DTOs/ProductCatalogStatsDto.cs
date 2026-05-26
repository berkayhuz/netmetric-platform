// <copyright file="ProductCatalogStatsDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class ProductCatalogStatsDto
{
    public required int ProductCount { get; init; }
    public required int ActiveProductCount { get; init; }
    public required int CategoryCount { get; init; }
    public required int ActiveCategoryCount { get; init; }
    public required int PriceListCount { get; init; }
    public required int ActivePriceListCount { get; init; }
    public required int DiscountMatrixCount { get; init; }
    public required int ActiveDiscountMatrixCount { get; init; }
    public required int ProductBindingCount { get; init; }
    public required int ActiveProductBindingCount { get; init; }
}
