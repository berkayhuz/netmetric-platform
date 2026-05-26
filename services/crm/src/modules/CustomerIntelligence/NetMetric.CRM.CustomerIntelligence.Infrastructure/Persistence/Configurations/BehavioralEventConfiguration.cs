// <copyright file="BehavioralEventConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence.Configurations;

public sealed class BehavioralEventConfiguration : IEntityTypeConfiguration<BehavioralEvent>
{
    public void Configure(EntityTypeBuilder<BehavioralEvent> builder)
    {
        builder.ToTable("BehavioralEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Source).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EventName).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SubjectType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.IdentityKey).HasMaxLength(256);
        builder.Property(x => x.Channel).HasMaxLength(64);
        builder.Property(x => x.PropertiesJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId, x.OccurredAtUtc });
    }
}
