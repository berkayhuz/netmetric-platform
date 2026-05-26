// <copyright file="CatalogProduct.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ProductCatalog.Domain.Common;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

public class CatalogProduct : AuditableEntity, ICatalogItemEntity
{
    public const string DefaultCurrencyCode = "USD";

    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? CategoryId { get; private set; }
    public CatalogCategory? Category { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public string CurrencyCode { get; private set; } = DefaultCurrencyCode;
    public decimal DefaultDiscountRate { get; private set; }
    public decimal DefaultTaxRate { get; private set; }
    public Guid? PrimaryImageMediaAssetId { get; private set; }
    public string? PrimaryImageUrl { get; private set; }

    private CatalogProduct() { }

    public CatalogProduct(
        string code,
        string name,
        string? description = null,
        Guid? categoryId = null,
        decimal? unitPrice = null,
        string? currencyCode = null,
        decimal defaultDiscountRate = 0,
        decimal defaultTaxRate = 0)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        SetCategory(categoryId);
        SetPricing(unitPrice, currencyCode);
        SetDefaultRates(defaultDiscountRate, defaultTaxRate);
    }

    public void Update(string code, string name, string? description)
    {
        Code = Guard.AgainstNullOrWhiteSpace(code);
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }

    public void Update(
        string code,
        string name,
        string? description,
        Guid? categoryId,
        decimal? unitPrice,
        string? currencyCode,
        decimal defaultDiscountRate,
        decimal defaultTaxRate)
    {
        Update(code, name, description);
        SetCategory(categoryId);
        SetPricing(unitPrice, currencyCode);
        SetDefaultRates(defaultDiscountRate, defaultTaxRate);
    }

    public void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public void SetPricing(decimal? unitPrice, string? currencyCode)
    {
        if (unitPrice.HasValue && unitPrice.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitPrice), "Unit price cannot be negative.");
        }

        UnitPrice = unitPrice;
        CurrencyCode = NormalizeCurrencyCode(currencyCode);
    }

    public void SetDefaultRates(decimal defaultDiscountRate, decimal defaultTaxRate)
    {
        if (defaultDiscountRate < 0 || defaultDiscountRate > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultDiscountRate),
                "Default discount rate must be between 0 and 100.");
        }

        if (defaultTaxRate < 0 || defaultTaxRate > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(defaultTaxRate),
                "Default tax rate must be between 0 and 100.");
        }

        DefaultDiscountRate = defaultDiscountRate;
        DefaultTaxRate = defaultTaxRate;
    }

    public void SetPrimaryImage(Guid? mediaAssetId, string? imageUrl)
    {
        PrimaryImageMediaAssetId = mediaAssetId;
        PrimaryImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();
    }

    private static string NormalizeCurrencyCode(string? currencyCode)
    {
        if (string.IsNullOrWhiteSpace(currencyCode))
        {
            return DefaultCurrencyCode;
        }

        return currencyCode.Trim().ToUpperInvariant();
    }
}
