// <copyright file="QuoteConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes");
        builder.Property(x => x.QuoteNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
        builder.Property(x => x.SubTotal).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountTotal).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxTotal).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.GrandTotal).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 2).HasColumnType("decimal(18,6)");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.QuoteNumber }).IsUnique();
    }
}
