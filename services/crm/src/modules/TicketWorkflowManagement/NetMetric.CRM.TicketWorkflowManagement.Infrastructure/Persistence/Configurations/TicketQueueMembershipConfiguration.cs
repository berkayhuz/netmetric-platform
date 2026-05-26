// <copyright file="TicketQueueMembershipConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketQueueMembershipConfiguration : IEntityTypeConfiguration<TicketQueueMembership>
{
    public void Configure(EntityTypeBuilder<TicketQueueMembership> builder)
    {
        builder.ToTable("TicketQueueMemberships");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Role).HasMaxLength(64).IsRequired();
        builder.HasOne(x => x.Queue).WithMany(x => x.Memberships).HasForeignKey(x => x.QueueId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.QueueId, x.UserId }).IsUnique();
    }
}
