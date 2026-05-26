// <copyright file="UserConsentConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Consents;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class UserConsentConfiguration : IEntityTypeConfiguration<UserConsent>
{
    public void Configure(EntityTypeBuilder<UserConsent> builder)
    {
        builder.ToTable("account_user_consents");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.UserId).HasConversion(StrongIdConversions.UserId).IsRequired();
        builder.Property(x => x.ConsentType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.DecidedAt).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1024);
        builder.Property(x => x.VersionToken).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.UserId, x.ConsentType, x.DecidedAt });
    }
}
