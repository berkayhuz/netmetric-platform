// <copyright file="SlaPolicyConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ServiceManagement;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence.Configurations;

public sealed class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("SlaPolicies");
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
