// <copyright file="ChurnPreventionCaseConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.ChurnPrevention;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence.Configurations;

public sealed class ChurnPreventionCaseConfiguration : IEntityTypeConfiguration<ChurnPreventionCase>
{
    public void Configure(EntityTypeBuilder<ChurnPreventionCase> builder)
    {
        builder.ToTable("ChurnPreventionCase");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
