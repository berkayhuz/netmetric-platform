// <copyright file="UserProfileConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Profiles;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("account_user_profiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(220).IsRequired();
        builder.Property(x => x.PhoneNumber).HasMaxLength(32);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.AvatarMediaAssetId);
        builder.Property(x => x.JobTitle).HasMaxLength(120);
        builder.Property(x => x.Department).HasMaxLength(120);
        builder.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Culture).HasMaxLength(20).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
    }
}
