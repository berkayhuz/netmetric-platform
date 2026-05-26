// <copyright file="DealConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class DealConfiguration : IEntityTypeConfiguration<Deal>
{
    public void Configure(EntityTypeBuilder<Deal> builder)
    {
        builder.ToTable("Deals");
        builder.Property(x => x.DealCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.HasIndex(x => new { x.TenantId, x.DealCode }).IsUnique();
    }
}
