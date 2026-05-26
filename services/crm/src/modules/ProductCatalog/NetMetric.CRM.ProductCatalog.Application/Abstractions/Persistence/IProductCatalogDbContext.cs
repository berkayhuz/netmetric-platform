// <copyright file="IProductCatalogDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.DiscountMatrices;
using NetMetric.CRM.ProductCatalog.Domain.Entities.PriceLists;
using NetMetric.CRM.ProductCatalog.Domain.Entities.ProductBindings;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;
using NetMetric.Repository;

namespace NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;

public interface IProductCatalogDbContext : IUnitOfWork
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    DbSet<CatalogProduct> Products { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<ProductCatalogMediaAsset> MediaAssets { get; }
    DbSet<CatalogCategory> Categories { get; }
    DbSet<PriceList> PriceLists { get; }
    DbSet<DiscountMatrix> DiscountMatrices { get; }
    DbSet<ProductBinding> ProductBindings { get; }
}
