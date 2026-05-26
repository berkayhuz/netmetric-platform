// <copyright file="TicketSlaInstanceConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.TicketSlaManagement.Domain.Entities;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketSlaInstanceConfiguration : IEntityTypeConfiguration<TicketSlaInstance>
{
    public void Configure(EntityTypeBuilder<TicketSlaInstance> builder)
    {
        builder.ToTable("TicketSlaInstances");
        builder.HasIndex(x => new { x.TenantId, x.TicketId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasOne(x => x.SlaPolicy)
            .WithMany()
            .HasForeignKey(x => x.SlaPolicyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
