// <copyright file="ToolDefinitionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Tools.Domain.Entities;

namespace NetMetric.Tools.Persistence.Configurations;

public sealed class ToolDefinitionConfiguration : IEntityTypeConfiguration<ToolDefinition>
{
    public void Configure(EntityTypeBuilder<ToolDefinition> builder)
    {
        builder.ToTable("tools_catalog_items");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Slug).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(512).IsRequired();
        builder.Property(x => x.SeoTitle).HasMaxLength(160).IsRequired();
        builder.Property(x => x.SeoDescription).HasMaxLength(320).IsRequired();
        builder.Property(x => x.AcceptedMimeTypesCsv).HasMaxLength(512).IsRequired();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.CategoryId, x.IsEnabled });
    }
}
