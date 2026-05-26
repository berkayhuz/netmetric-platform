// <copyright file="OpportunityConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class OpportunityConfiguration : IEntityTypeConfiguration<Opportunity>
{
    public void Configure(EntityTypeBuilder<Opportunity> builder)
    {
        builder.ToTable("Opportunities");
        builder.Property(x => x.OpportunityCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Probability).HasPrecision(18, 2).HasColumnType("decimal(5,2)");
        builder.Property(x => x.EstimatedAmount).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ExpectedRevenue).HasPrecision(18, 2).HasColumnType("decimal(18,2)");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.OpportunityCode }).IsUnique();
    }
}
