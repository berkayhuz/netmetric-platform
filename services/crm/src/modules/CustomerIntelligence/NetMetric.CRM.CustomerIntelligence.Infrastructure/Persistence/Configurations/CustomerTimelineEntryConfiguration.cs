// <copyright file="CustomerTimelineEntryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence.Configurations;

public sealed class CustomerTimelineEntryConfiguration : IEntityTypeConfiguration<CustomerTimelineEntry>
{
    public void Configure(EntityTypeBuilder<CustomerTimelineEntry> builder)
    {
        builder.ToTable("CustomerTimelineEntrys");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SubjectType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Channel).HasMaxLength(64);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId, x.OccurredAtUtc });
    }
}
