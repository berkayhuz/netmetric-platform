// <copyright file="TicketQueueConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketQueueConfiguration : IEntityTypeConfiguration<TicketQueue>
{
    public void Configure(EntityTypeBuilder<TicketQueue> builder)
    {
        builder.ToTable("TicketQueues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique();
    }
}
