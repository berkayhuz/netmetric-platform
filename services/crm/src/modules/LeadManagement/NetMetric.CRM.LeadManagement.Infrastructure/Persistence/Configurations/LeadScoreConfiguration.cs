// <copyright file="LeadScoreConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Persistence.Configurations;

public sealed class LeadScoreConfiguration : IEntityTypeConfiguration<LeadScore>
{
    public void Configure(EntityTypeBuilder<LeadScore> builder)
    {
        builder.ToTable("LeadScores");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score).HasPrecision(9, 2);
        builder.Property(x => x.FitScoreDelta).HasPrecision(9, 2);
        builder.Property(x => x.EngagementScoreDelta).HasPrecision(9, 2);
        builder.Property(x => x.ScoreReason).HasMaxLength(500);
        builder.Property(x => x.RuleId).HasMaxLength(128);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.LeadId, x.CalculatedAt });

        builder.HasOne(x => x.Lead)
            .WithMany()
            .HasForeignKey(x => x.LeadId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
