// <copyright file="AiInsightRunConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Infrastructure.Persistence.Configurations;

public sealed class AiInsightRunConfiguration : IEntityTypeConfiguration<AiInsightRun>
{
    public void Configure(EntityTypeBuilder<AiInsightRun> builder)
    {
        builder.ToTable("AiInsightRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceEntityType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
    }
}
