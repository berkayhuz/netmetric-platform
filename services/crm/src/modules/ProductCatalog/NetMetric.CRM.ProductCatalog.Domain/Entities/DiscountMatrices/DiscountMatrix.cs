// <copyright file="DiscountMatrix.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ProductCatalog.Domain.Common;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.ProductCatalog.Domain.Entities.DiscountMatrices;

public class DiscountMatrix : AuditableEntity, ICatalogItemEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    private DiscountMatrix() { }

    public DiscountMatrix(string code, string name, string? description = null)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Update(string code, string name, string? description)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
}
