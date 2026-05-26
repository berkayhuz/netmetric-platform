// <copyright file="ProductBundleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class ProductBundleConfiguration : IEntityTypeConfiguration<ProductBundle>
{
    public void Configure(EntityTypeBuilder<ProductBundle> builder)
    {
        builder.ToTable("ProductBundles", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.Segment).HasMaxLength(128);
        builder.Property(x => x.Industry).HasMaxLength(128);
        builder.Property(x => x.DiscountRate).HasPrecision(18, 2);
        builder.Property(x => x.MinimumBudget).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasMany(x => x.Items).WithOne(x => x.ProductBundle).HasForeignKey(x => x.ProductBundleId).OnDelete(DeleteBehavior.Cascade);
    }
}
