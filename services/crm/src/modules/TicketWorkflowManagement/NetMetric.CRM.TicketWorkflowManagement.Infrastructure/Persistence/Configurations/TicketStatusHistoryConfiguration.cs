// <copyright file="TicketStatusHistoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketStatusHistoryConfiguration : IEntityTypeConfiguration<TicketStatusHistory>
{
    public void Configure(EntityTypeBuilder<TicketStatusHistory> builder)
    {
        builder.ToTable("TicketStatusHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PreviousStatus).HasMaxLength(64).IsRequired();
        builder.Property(x => x.NewStatus).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Note).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.TicketId, x.ChangedAtUtc });
    }
}
