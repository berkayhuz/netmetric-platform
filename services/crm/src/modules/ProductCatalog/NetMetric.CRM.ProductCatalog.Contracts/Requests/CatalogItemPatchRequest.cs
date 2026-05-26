// <copyright file="CatalogItemPatchRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.Requests;

public sealed class CatalogItemPatchRequest
{
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsActive { get; init; }
    public Guid? CategoryId { get; init; }
    public decimal? UnitPrice { get; init; }
    public string? CurrencyCode { get; init; }
    public decimal? DefaultDiscountRate { get; init; }
    public decimal? DefaultTaxRate { get; init; }
}
