// <copyright file="SubscriptionRecordConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Subscriptions;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionRecordConfiguration : IEntityTypeConfiguration<SubscriptionRecord>
{
    public void Configure(EntityTypeBuilder<SubscriptionRecord> builder)
    {
        builder.ToTable("SubscriptionRecord");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.BillingPeriod).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Mrr).HasPrecision(18, 2);
        builder.Property(x => x.Arr).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CustomerId });
        builder.HasIndex(x => new { x.TenantId, x.CompanyId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}
