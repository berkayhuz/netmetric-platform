// <copyright file="UserSessionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Sessions;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("account_user_sessions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(160);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.ApproximateLocation).HasMaxLength(160);
        builder.Property(x => x.RevocationReason).HasMaxLength(120);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.LastSeenAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.LastSeenAt });
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.RevokedAt, x.ExpiresAt });
    }
}
