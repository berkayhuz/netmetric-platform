// <copyright file="CatalogItemUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.Requests;

public sealed class CatalogItemUpsertRequest
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? UnitPrice { get; init; }
    public string CurrencyCode { get; init; } = "USD";
    public decimal DefaultDiscountRate { get; init; }
    public decimal DefaultTaxRate { get; init; }
}
