// <copyright file="GlobalTrashItemConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class GlobalTrashItemConfiguration : IEntityTypeConfiguration<GlobalTrashItem>
{
    public void Configure(EntityTypeBuilder<GlobalTrashItem> builder)
    {
        builder.ToTable("GlobalTrashItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(512);
        builder.Property(x => x.SourceModule).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OriginalRoute).HasMaxLength(256);
        builder.Property(x => x.DeletedByDisplayName).HasMaxLength(256);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.AuditCorrelationId).HasMaxLength(128);
        builder.Property(x => x.RowVersion).IsConcurrencyToken();

        builder.HasIndex(x => new { x.TenantId, x.Status, x.ExpiresAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.DeletedAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.Status })
            .IsUnique()
            .HasFilter("[Status] = 'active'");
    }
}
