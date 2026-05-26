// <copyright file="TicketAssignmentHistoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Entities;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketAssignmentHistoryConfiguration : IEntityTypeConfiguration<TicketAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<TicketAssignmentHistory> builder)
    {
        builder.ToTable("TicketAssignmentHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.TicketId, x.ChangedAtUtc });
    }
}
