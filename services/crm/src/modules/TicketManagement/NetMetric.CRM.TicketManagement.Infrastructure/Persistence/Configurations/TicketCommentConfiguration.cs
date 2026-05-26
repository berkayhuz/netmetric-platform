// <copyright file="TicketCommentConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Support;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketCommentConfiguration : IEntityTypeConfiguration<TicketComment>
{
    public void Configure(EntityTypeBuilder<TicketComment> builder)
    {
        builder.ToTable("TicketComments", "ticketing");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comment).HasMaxLength(4000).IsRequired();
    }
}
