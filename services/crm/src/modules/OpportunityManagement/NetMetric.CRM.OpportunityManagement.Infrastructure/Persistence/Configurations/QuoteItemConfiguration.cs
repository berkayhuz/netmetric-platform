// <copyright file="QuoteItemConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class QuoteItemConfiguration : IEntityTypeConfiguration<QuoteItem>
{
    public void Configure(EntityTypeBuilder<QuoteItem> builder)
    {
        builder.ToTable("QuoteItems");
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountRate).HasPrecision(18, 2).HasColumnType("decimal(5,2)");
        builder.Property(x => x.TaxRate).HasPrecision(18, 2).HasColumnType("decimal(5,2)");
        builder.Property(x => x.LineTotal).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
    }
}
