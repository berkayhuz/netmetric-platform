// <copyright file="IdentityProfileConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence.Configurations;

public sealed class IdentityProfileConfiguration : IEntityTypeConfiguration<IdentityProfile>
{
    public void Configure(EntityTypeBuilder<IdentityProfile> builder)
    {
        builder.ToTable("IdentityProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SubjectType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.IdentityType).HasMaxLength(64).IsRequired();
        builder.Property(x => x.IdentityValue).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ResolutionNotes).HasMaxLength(1024);
        builder.HasIndex(x => new { x.TenantId, x.SubjectType, x.SubjectId, x.IdentityType, x.IdentityValue }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
