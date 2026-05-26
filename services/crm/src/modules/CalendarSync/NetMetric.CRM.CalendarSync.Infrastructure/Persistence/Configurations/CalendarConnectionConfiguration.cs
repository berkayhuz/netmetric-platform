// <copyright file="CalendarConnectionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CalendarSync.Domain.Entities;

namespace NetMetric.CRM.CalendarSync.Infrastructure.Persistence.Configurations;

public sealed class CalendarConnectionConfiguration : IEntityTypeConfiguration<CalendarConnection>
{
    public void Configure(EntityTypeBuilder<CalendarConnection> builder)
    {
        builder.ToTable("CalendarConnections");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CalendarIdentifier).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SecretReference).HasMaxLength(300).IsRequired();
    }
}
