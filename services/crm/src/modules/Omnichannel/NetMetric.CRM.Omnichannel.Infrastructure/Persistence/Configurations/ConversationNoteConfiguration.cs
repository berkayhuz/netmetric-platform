// <copyright file="ConversationNoteConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Omnichannel.Domain.Entities;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Persistence.Configurations;

public sealed class ConversationNoteConfiguration : IEntityTypeConfiguration<ConversationNote>
{
    public void Configure(EntityTypeBuilder<ConversationNote> builder)
    {
        builder.ToTable("OmnichannelConversationNotes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AuthorDisplayName).HasMaxLength(160).IsRequired();
        builder.Property(x => x.NoteText).HasMaxLength(2000).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ConversationId, x.CreatedAtUtc });
    }
}
