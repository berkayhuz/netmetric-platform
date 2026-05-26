// <copyright file="WorkTaskConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkManagement.Domain.Entities;

namespace NetMetric.CRM.WorkManagement.Infrastructure.Persistence.Configurations;

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("WorkTasks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.CompletionNote).HasMaxLength(1000);
    }
}
