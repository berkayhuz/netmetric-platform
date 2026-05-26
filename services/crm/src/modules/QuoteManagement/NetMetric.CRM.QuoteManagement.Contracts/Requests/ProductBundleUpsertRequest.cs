// <copyright file="ProductBundleUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class ProductBundleUpsertRequest
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string? Segment { get; set; }
    public string? Industry { get; set; }
    public decimal DiscountRate { get; set; }
    public decimal? MinimumBudget { get; set; }
    public List<ProductBundleItemRequest> Items { get; set; } = new();
    public string? RowVersion { get; set; }
}
