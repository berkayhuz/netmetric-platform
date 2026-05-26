// <copyright file="UserNotificationStateConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Notifications;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class UserNotificationStateConfiguration : IEntityTypeConfiguration<UserNotificationState>
{
    public void Configure(EntityTypeBuilder<UserNotificationState> builder)
    {
        builder.ToTable("account_user_notification_states");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.NotificationId).IsRequired();
        builder.Property(x => x.IsRead).IsRequired();
        builder.Property(x => x.ReadAt);
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedAt);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.NotificationId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.IsDeleted });
    }
}
