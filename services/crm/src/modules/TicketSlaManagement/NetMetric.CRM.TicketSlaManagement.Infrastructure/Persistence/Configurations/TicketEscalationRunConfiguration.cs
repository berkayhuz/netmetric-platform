// <copyright file="TicketEscalationRunConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketEscalationRunConfiguration : IEntityTypeConfiguration<TicketEscalationRun>
{
    public void Configure(EntityTypeBuilder<TicketEscalationRun> builder)
    {
        builder.ToTable("TicketEscalationRuns");
        builder.Property(x => x.Note).HasMaxLength(500).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.TicketId, x.ExecutedAtUtc });
    }
}
