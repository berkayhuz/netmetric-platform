// <copyright file="CatalogProductTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

namespace NetMetric.CRM.ProductCatalog.UnitTests;

public sealed class CatalogProductTests
{
    [Fact]
    public void Constructor_Should_Set_Required_Fields()
    {
        var categoryId = Guid.NewGuid();
        var entity = new CatalogProduct("CODE-1", "Name 1", "Desc", categoryId, 199.99m, "eur");
        entity.Code.Should().Be("CODE-1");
        entity.Name.Should().Be("Name 1");
        entity.Description.Should().Be("Desc");
        entity.CategoryId.Should().Be(categoryId);
        entity.UnitPrice.Should().Be(199.99m);
        entity.CurrencyCode.Should().Be("EUR");
        entity.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Update_Should_Modify_Pricing_And_Category()
    {
        var entity = new CatalogProduct("CODE-1", "Name 1", "Desc", null, 10m, "USD");
        var categoryId = Guid.NewGuid();

        entity.Update("CODE-2", "Name 2", "Desc 2", categoryId, 12.5m, "try", 0m, 0m);

        entity.Code.Should().Be("CODE-2");
        entity.Name.Should().Be("Name 2");
        entity.Description.Should().Be("Desc 2");
        entity.CategoryId.Should().Be(categoryId);
        entity.UnitPrice.Should().Be(12.5m);
        entity.CurrencyCode.Should().Be("TRY");
    }

    [Fact]
    public void SetPricing_Should_Throw_When_UnitPrice_Is_Negative()
    {
        var entity = new CatalogProduct("CODE-1", "Name 1", "Desc");

        var act = () => entity.SetPricing(-1m, "USD");

        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
