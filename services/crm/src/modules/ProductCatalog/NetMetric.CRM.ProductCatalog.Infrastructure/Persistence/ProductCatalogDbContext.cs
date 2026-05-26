// <copyright file="ProductCatalogDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.DiscountMatrices;
using NetMetric.CRM.ProductCatalog.Domain.Entities.PriceLists;
using NetMetric.CRM.ProductCatalog.Domain.Entities.ProductBindings;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;
using NetMetric.CRM.ProductCatalog.Infrastructure.Persistence.Configurations;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Persistence;

public sealed class ProductCatalogDbContext : AppDbContext, IProductCatalogDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public ProductCatalogDbContext(
        DbContextOptions<ProductCatalogDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<CatalogProduct> Products => Set<CatalogProduct>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductCatalogMediaAsset> MediaAssets => Set<ProductCatalogMediaAsset>();
    public DbSet<CatalogCategory> Categories => Set<CatalogCategory>();
    public DbSet<PriceList> PriceLists => Set<PriceList>();
    public DbSet<DiscountMatrix> DiscountMatrices => Set<DiscountMatrix>();
    public DbSet<ProductBinding> ProductBindings => Set<ProductBinding>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CatalogProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductImageConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCatalogMediaAssetConfiguration());
        modelBuilder.ApplyConfiguration(new CatalogCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new PriceListConfiguration());
        modelBuilder.ApplyConfiguration(new DiscountMatrixConfiguration());
        modelBuilder.ApplyConfiguration(new ProductBindingConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
