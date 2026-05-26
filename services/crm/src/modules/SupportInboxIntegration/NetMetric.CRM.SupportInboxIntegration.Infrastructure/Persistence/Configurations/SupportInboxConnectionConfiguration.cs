// <copyright file="SupportInboxConnectionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence.Configurations;

public sealed class SupportInboxConnectionConfiguration : IEntityTypeConfiguration<SupportInboxConnection>
{
    public void Configure(EntityTypeBuilder<SupportInboxConnection> builder)
    {
        builder.ToTable("SupportInboxConnections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EmailAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Host).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Username).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretReference).HasMaxLength(300).IsRequired();
    }
}
