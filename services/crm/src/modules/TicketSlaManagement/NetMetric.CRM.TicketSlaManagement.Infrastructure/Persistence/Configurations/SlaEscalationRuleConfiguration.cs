// <copyright file="SlaEscalationRuleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence.Configurations;

public sealed class SlaEscalationRuleConfiguration : IEntityTypeConfiguration<SlaEscalationRule>
{
    public void Configure(EntityTypeBuilder<SlaEscalationRule> builder)
    {
        builder.ToTable("SlaEscalationRules");
        builder.HasOne(x => x.SlaPolicy)
            .WithMany()
            .HasForeignKey(x => x.SlaPolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
