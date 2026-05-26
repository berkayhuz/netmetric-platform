// <copyright file="TrustedDeviceConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Devices;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class TrustedDeviceConfiguration : IEntityTypeConfiguration<TrustedDevice>
{
    public void Configure(EntityTypeBuilder<TrustedDevice> builder)
    {
        builder.ToTable("account_trusted_devices");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.DeviceTokenHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.TrustedAt).IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.RevocationReason).HasMaxLength(120);
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ExpiresAt });
        builder.HasIndex(x => new { x.TenantId, x.UserId, x.DeviceTokenHash }).IsUnique();
    }
}
