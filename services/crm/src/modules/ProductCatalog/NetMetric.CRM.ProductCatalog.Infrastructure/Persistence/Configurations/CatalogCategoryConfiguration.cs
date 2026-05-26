// <copyright file="CatalogCategoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Categories;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Persistence.Configurations;

public sealed class CatalogCategoryConfiguration : IEntityTypeConfiguration<CatalogCategory>
{
    public void Configure(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.ToTable("CatalogCategory");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ImageUrl).HasMaxLength(2048);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
