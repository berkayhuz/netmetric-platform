// <copyright file="TagMapConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Tagging;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class TagMapConfiguration : IEntityTypeConfiguration<TagMap>
{
    public void Configure(EntityTypeBuilder<TagMap> builder)
    {
        builder.ToTable("TagMaps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Property(x => x.EntityType).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.TagId, x.EntityType, x.EntityId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId })
            .HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.Tag)
            .WithMany(x => x.TagMaps)
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
