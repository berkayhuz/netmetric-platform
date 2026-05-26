// <copyright file="MeetingScheduleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Infrastructure.Persistence.Configurations;

public sealed class MeetingScheduleConfiguration : IEntityTypeConfiguration<MeetingSchedule>
{
    public void Configure(EntityTypeBuilder<MeetingSchedule> builder)
    {
        builder.ToTable("MeetingSchedules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.OrganizerEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.AttendeeSummary).HasMaxLength(2000).IsRequired();
    }
}
