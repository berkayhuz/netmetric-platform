// <copyright file="ProductCategory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Catalog;

public class ProductCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public ProductCategory? ParentCategory { get; set; }
}
