// <copyright file="ToolArtifactConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Tools.Domain.Entities;

namespace NetMetric.Tools.Persistence.Configurations;

public sealed class ToolArtifactConfiguration : IEntityTypeConfiguration<ToolArtifact>
{
    public void Configure(EntityTypeBuilder<ToolArtifact> builder)
    {
        builder.ToTable("tool_artifacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MimeType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(512).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.ChecksumSha256).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.OwnerUserId, x.CreatedAtUtc });
        builder.HasIndex(x => x.ToolRunId);
        builder.HasOne<ToolRun>()
            .WithMany()
            .HasForeignKey(x => x.ToolRunId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
