// <copyright file="ProductBundleItem.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.QuoteManagement.Domain.Entities;

public sealed class ProductBundleItem : AuditableEntity
{
    public Guid ProductBundleId { get; set; }
    public ProductBundle ProductBundle { get; set; } = null!;
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public bool IsOptional { get; set; }
}
