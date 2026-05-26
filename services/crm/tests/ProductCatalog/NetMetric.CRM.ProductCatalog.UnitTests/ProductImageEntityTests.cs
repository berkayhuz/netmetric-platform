// <copyright file="ProductImageEntityTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

namespace NetMetric.CRM.ProductCatalog.UnitTests;

public sealed class ProductImageEntityTests
{
    [Fact]
    public void CatalogProduct_CanStore_PrimaryImageReference()
    {
        var product = new CatalogProduct("P-1", "Product");
        var mediaId = Guid.NewGuid();
        product.SetPrimaryImage(mediaId, "https://cdn.netmetric.net/media/p.png");
        product.PrimaryImageMediaAssetId.Should().Be(mediaId);
        product.PrimaryImageUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void CatalogCategory_CanStore_ImageReference()
    {
        var category = new CatalogCategory("C-1", "Category");
        var mediaId = Guid.NewGuid();
        category.SetImage(mediaId, "https://cdn.netmetric.net/media/c.png");
        category.ImageMediaAssetId.Should().Be(mediaId);
        category.ImageUrl.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ProductImage_UpdateMetadata_ChangesSortAndAltText()
    {
        var image = new ProductImage(Guid.NewGuid(), Guid.NewGuid(), 0, false, null);
        image.UpdateMetadata(3, "front view");
        image.SortOrder.Should().Be(3);
        image.AltText.Should().Be("front view");
    }
}
