// <copyright file="ProductRuleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class ProductRuleConfiguration : IEntityTypeConfiguration<ProductRule>
{
    public void Configure(EntityTypeBuilder<ProductRule> builder)
    {
        builder.ToTable("ProductRules", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RuleType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Severity).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(512).IsRequired();
        builder.Property(x => x.MaximumDiscountRate).HasPrecision(18, 2);
        builder.Property(x => x.CriteriaJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
