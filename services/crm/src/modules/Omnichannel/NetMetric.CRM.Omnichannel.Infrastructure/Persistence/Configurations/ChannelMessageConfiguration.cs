// <copyright file="ChannelMessageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Persistence.Configurations;

public sealed class ChannelMessageConfiguration : IEntityTypeConfiguration<ChannelMessage>
{
    public void Configure(EntityTypeBuilder<ChannelMessage> builder)
    {
        builder.ToTable("OmnichannelMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Direction).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Body).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.ExternalMessageId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SenderDisplayName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExternalMessageId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.ConversationId, x.SentAtUtc });
    }
}
