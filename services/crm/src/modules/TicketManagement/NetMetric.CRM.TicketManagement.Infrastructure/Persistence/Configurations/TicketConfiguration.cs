// <copyright file="TicketConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Support;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("Tickets", "ticketing");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber).HasMaxLength(32).IsRequired();
        builder.Property(x => x.Subject).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(4000);

        builder.HasIndex(x => new { x.TenantId, x.TicketNumber }).IsUnique();

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Ticket)
            .HasForeignKey(x => x.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
