// <copyright file="SupportInboxMessageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence.Configurations;

public sealed class SupportInboxMessageConfiguration : IEntityTypeConfiguration<SupportInboxMessage>
{
    public void Configure(EntityTypeBuilder<SupportInboxMessage> builder)
    {
        builder.ToTable("SupportInboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalMessageId).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FromAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ProcessingError).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.ConnectionId, x.ExternalMessageId }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
