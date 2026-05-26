// <copyright file="UserPreferenceConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Preferences;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class UserPreferenceConfiguration : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.ToTable("account_user_preferences");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.Theme).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Language).HasMaxLength(20).IsRequired();
        builder.Property(x => x.TimeZone).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DateFormat).HasMaxLength(40).IsRequired();
        builder.Property(x => x.PostLoginDestination).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.FaviconUrl).HasMaxLength(2048);
        builder.Property(x => x.CrmDashboardPreferencesJson).HasMaxLength(200000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId }).IsUnique();
    }
}
