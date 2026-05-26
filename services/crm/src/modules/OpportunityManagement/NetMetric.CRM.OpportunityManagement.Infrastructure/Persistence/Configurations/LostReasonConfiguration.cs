// <copyright file="LostReasonConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class LostReasonConfiguration : IEntityTypeConfiguration<LostReason>
{
    public void Configure(EntityTypeBuilder<LostReason> builder)
    {
        builder.ToTable("LostReasons");
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique();
    }
}
