// <copyright file="QuoteConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> builder)
    {
        builder.ToTable("Quotes", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuoteNumber).HasMaxLength(64).IsRequired();
        builder.Property(x => x.ProposalTitle).HasMaxLength(256);
        builder.Property(x => x.ProposalSummary).HasMaxLength(2000);
        builder.Property(x => x.TermsAndConditions).HasMaxLength(4000);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 6);
        builder.Property(x => x.SubTotal).HasPrecision(18, 2);
        builder.Property(x => x.DiscountTotal).HasPrecision(18, 2);
        builder.Property(x => x.TaxTotal).HasPrecision(18, 2);
        builder.Property(x => x.GrandTotal).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.QuoteNumber }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasMany(x => x.Items).WithOne(x => x.Quote).HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.ParentQuote).WithMany().HasForeignKey(x => x.ParentQuoteId).OnDelete(DeleteBehavior.Restrict);
    }
}
