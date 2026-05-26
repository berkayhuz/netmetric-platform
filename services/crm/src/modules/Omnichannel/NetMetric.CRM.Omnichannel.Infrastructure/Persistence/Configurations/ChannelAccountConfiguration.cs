// <copyright file="ChannelAccountConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Persistence.Configurations;

public sealed class ChannelAccountConfiguration : IEntityTypeConfiguration<ChannelAccount>
{
    public void Configure(EntityTypeBuilder<ChannelAccount> builder)
    {
        builder.ToTable("OmnichannelAccounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ExternalAccountId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretReference).HasMaxLength(300).IsRequired();
        builder.Property(x => x.RoutingKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ProviderCredentialId);
        builder.Property(x => x.IsActive).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.ProviderCredentialId, x.ExternalAccountId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ProviderCredentialId }).HasFilter("[IsDeleted] = 0");
    }
}
