// <copyright file="CalendarEventBridgeConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CalendarSync.Domain.Entities;

namespace NetMetric.CRM.CalendarSync.Infrastructure.Persistence.Configurations;

public sealed class CalendarEventBridgeConfiguration : IEntityTypeConfiguration<CalendarEventBridge>
{
    public void Configure(EntityTypeBuilder<CalendarEventBridge> builder)
    {
        builder.ToTable("CalendarEventBridges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalEventId).HasMaxLength(200).IsRequired();
    }
}
