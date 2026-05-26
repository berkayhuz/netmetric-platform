// <copyright file="ProductImageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImage");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AltText).HasMaxLength(256);
        builder.HasIndex(x => new { x.TenantId, x.ProductId, x.IsPrimary }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ProductId, x.SortOrder }).HasFilter("[IsDeleted] = 0");
    }
}
