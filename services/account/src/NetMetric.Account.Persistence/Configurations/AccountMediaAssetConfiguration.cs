// <copyright file="AccountMediaAssetConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Profiles;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class AccountMediaAssetConfiguration : IEntityTypeConfiguration<AccountMediaAsset>
{
    public void Configure(EntityTypeBuilder<AccountMediaAsset> builder)
    {
        builder.ToTable("account_media_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.OwnerUserId).HasConversion(StrongIdConversions.UserId);
        builder.Property(x => x.CreatedByUserId).HasConversion(StrongIdConversions.UserId);
        builder.Property(x => x.Module).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Purpose).HasMaxLength(64).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.SafeFileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Extension).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Sha256Hash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StorageProvider).HasMaxLength(64).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(x => x.PublicUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Visibility).HasMaxLength(16).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.Module, x.Purpose, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.OwnerUserId });
    }
}
