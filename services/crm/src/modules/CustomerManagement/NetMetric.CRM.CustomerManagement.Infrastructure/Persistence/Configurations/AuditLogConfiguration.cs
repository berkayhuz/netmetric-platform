// <copyright file="AuditLogConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Auditing;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.Property(x => x.EntityName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ActionType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128);

        builder.HasIndex(x => new { x.TenantId, x.EntityName, x.EntityId, x.ChangedAt });
        builder.HasIndex(x => new { x.TenantId, x.ChangedAt });
    }
}
