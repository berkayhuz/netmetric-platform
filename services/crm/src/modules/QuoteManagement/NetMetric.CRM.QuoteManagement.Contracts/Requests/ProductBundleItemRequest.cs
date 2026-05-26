// <copyright file="ProductBundleItemRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Requests;

public sealed class ProductBundleItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsOptional { get; set; }
}
