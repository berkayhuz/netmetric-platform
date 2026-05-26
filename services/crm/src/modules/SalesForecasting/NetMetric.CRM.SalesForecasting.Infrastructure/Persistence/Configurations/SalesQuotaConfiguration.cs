// <copyright file="SalesQuotaConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.SalesForecasting.Domain.Entities;

namespace NetMetric.CRM.SalesForecasting.Infrastructure.Persistence.Configurations;

internal sealed class SalesQuotaConfiguration : IEntityTypeConfiguration<SalesQuota>
{
    public void Configure(EntityTypeBuilder<SalesQuota> builder)
    {
        builder.ToTable("SalesQuotas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CurrencyCode).HasMaxLength(8).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.PeriodStart, x.PeriodEnd, x.OwnerUserId }).IsUnique();
    }
}
