// <copyright file="ChannelConversationConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Persistence.Configurations;

public sealed class ChannelConversationConfiguration : IEntityTypeConfiguration<ChannelConversation>
{
    public void Configure(EntityTypeBuilder<ChannelConversation> builder)
    {
        builder.ToTable("OmnichannelConversations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Subject).HasMaxLength(180).IsRequired();
        builder.Property(x => x.CustomerDisplayName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ExternalConversationId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ExternalParticipantId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.AssignedUserDisplayName).HasMaxLength(160);
        builder.Property(x => x.UnreadCount).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.ExternalConversationId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.LastMessageAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.Status, x.LastMessageAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.AssignedUserId, x.LastMessageAtUtc }).HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Priority, x.LastMessageAtUtc });
    }
}
