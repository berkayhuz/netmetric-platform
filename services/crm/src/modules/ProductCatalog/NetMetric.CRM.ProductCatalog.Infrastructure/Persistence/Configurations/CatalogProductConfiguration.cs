// <copyright file="CatalogProductConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class CatalogProductConfiguration : IEntityTypeConfiguration<CatalogProduct>
{
    public void Configure(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.ToTable("CatalogProduct");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DefaultDiscountRate).HasColumnType("decimal(5,2)").HasDefaultValue(0);
        builder.Property(x => x.DefaultTaxRate).HasColumnType("decimal(5,2)").HasDefaultValue(0);
        builder.Property(x => x.PrimaryImageUrl).HasMaxLength(2048);
        builder.HasOne(x => x.Category)
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
