// <copyright file="TicketPriorityHistoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Support;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Persistence.Configurations;

public sealed class TicketPriorityHistoryConfiguration : IEntityTypeConfiguration<TicketPriorityHistory>
{
    public void Configure(EntityTypeBuilder<TicketPriorityHistory> builder)
    {
        builder.ToTable("TicketPriorityHistories", "ticketing");
        builder.HasKey(x => x.Id);
    }
}
