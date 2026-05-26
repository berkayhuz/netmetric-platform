// <copyright file="ProductCatalogLookupsDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class ProductCatalogLookupsDto
{
    public required IReadOnlyList<CatalogLookupItemDto> Products { get; init; }
    public required IReadOnlyList<CatalogLookupItemDto> Categories { get; init; }
    public required IReadOnlyList<CatalogLookupItemDto> PriceLists { get; init; }
    public required IReadOnlyList<CatalogLookupItemDto> DiscountMatrices { get; init; }
    public required IReadOnlyList<CatalogLookupItemDto> ProductBindings { get; init; }
    public required IReadOnlyList<string> Currencies { get; init; }
}
