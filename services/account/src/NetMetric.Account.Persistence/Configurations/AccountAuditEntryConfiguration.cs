// <copyright file="AccountAuditEntryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Audit;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class AccountAuditEntryConfiguration : IEntityTypeConfiguration<AccountAuditEntry>
{
    public void Configure(EntityTypeBuilder<AccountAuditEntry> builder)
    {
        builder.ToTable("account_audit_entries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.OccurredAt });
    }
}
