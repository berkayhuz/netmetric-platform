// <copyright file="ProductImage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

public sealed class ProductImage : AuditableEntity
{
    public Guid ProductId { get; private set; }
    public Guid MediaAssetId { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsPrimary { get; private set; }
    public string? AltText { get; private set; }

    private ProductImage() { }

    public ProductImage(Guid productId, Guid mediaAssetId, int sortOrder, bool isPrimary, string? altText)
    {
        ProductId = productId;
        MediaAssetId = mediaAssetId;
        SortOrder = sortOrder;
        IsPrimary = isPrimary;
        AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim();
    }

    public void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    public void UpdateMetadata(int sortOrder, string? altText)
    {
        SortOrder = sortOrder;
        AltText = string.IsNullOrWhiteSpace(altText) ? null : altText.Trim();
    }
}
