// <copyright file="CustomerRelationshipConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence.Configurations;

public sealed class CustomerRelationshipConfiguration : IEntityTypeConfiguration<CustomerRelationship>
{
    public void Configure(EntityTypeBuilder<CustomerRelationship> builder)
    {
        builder.ToTable("CustomerRelationships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceEntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.TargetEntityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RelationshipType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.SourceEntityType, x.SourceEntityId, x.TargetEntityType, x.TargetEntityId, x.RelationshipType }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
