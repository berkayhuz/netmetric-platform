// <copyright file="ProductCatalogSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class ProductCatalogSummaryDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }
    public Guid? CategoryId { get; init; }
    public string? CategoryCode { get; init; }
    public string? CategoryName { get; init; }
    public decimal? UnitPrice { get; init; }
    public required string CurrencyCode { get; init; }
    public decimal DefaultDiscountRate { get; init; }
    public decimal DefaultTaxRate { get; init; }
    public string? PrimaryImageUrl { get; init; }
}
