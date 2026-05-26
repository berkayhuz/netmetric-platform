// <copyright file="ProductCatalogMetaDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class ProductCatalogMetaDto
{
    public required string Module { get; init; }
    public required string Version { get; init; }
    public required IReadOnlyList<string> Resources { get; init; }
    public required IReadOnlyList<string> Features { get; init; }
}
