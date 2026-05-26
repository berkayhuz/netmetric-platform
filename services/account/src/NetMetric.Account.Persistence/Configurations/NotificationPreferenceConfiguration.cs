// <copyright file="NotificationPreferenceConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Notifications;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("account_notification_preferences");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.Channel).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.Channel, x.Category }).IsUnique();
    }
}
