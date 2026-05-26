// <copyright file="QuoteStatusHistoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class QuoteStatusHistoryConfiguration : IEntityTypeConfiguration<QuoteStatusHistory>
{
    public void Configure(EntityTypeBuilder<QuoteStatusHistory> builder)
    {
        builder.ToTable("QuoteStatusHistories", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Note).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.QuoteId, x.ChangedAt });
    }
}
