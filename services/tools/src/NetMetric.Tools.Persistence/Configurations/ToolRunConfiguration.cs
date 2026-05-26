// <copyright file="ToolRunConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Tools.Domain.Entities;

namespace NetMetric.Tools.Persistence.Configurations;

public sealed class ToolRunConfiguration : IEntityTypeConfiguration<ToolRun>
{
    public void Configure(EntityTypeBuilder<ToolRun> builder)
    {
        builder.ToTable("tool_runs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ToolSlug).HasMaxLength(64).IsRequired();
        builder.Property(x => x.InputSummaryJson).HasMaxLength(4000).IsRequired();
        builder.HasIndex(x => new { x.OwnerUserId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.OwnerUserId, x.ToolSlug });
    }
}
