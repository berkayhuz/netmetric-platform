// <copyright file="ProductBundleItemConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class ProductBundleItemConfiguration : IEntityTypeConfiguration<ProductBundleItem>
{
    public void Configure(EntityTypeBuilder<ProductBundleItem> builder)
    {
        builder.ToTable("ProductBundleItems", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.ProductBundleId, x.ProductId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
