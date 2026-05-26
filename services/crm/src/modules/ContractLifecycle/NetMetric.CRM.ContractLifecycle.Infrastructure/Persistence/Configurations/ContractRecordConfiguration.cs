// <copyright file="ContractRecordConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence.Configurations;

public sealed class ContractRecordConfiguration : IEntityTypeConfiguration<ContractRecord>
{
    public void Configure(EntityTypeBuilder<ContractRecord> builder)
    {
        builder.ToTable("ContractRecord");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.ContractNumber).HasMaxLength(64);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ContractValue).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.CompanyId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.RenewalDateUtc });
    }
}
