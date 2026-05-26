// <copyright file="AiAutomationPolicyConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Infrastructure.Persistence.Configurations;

public sealed class AiAutomationPolicyConfiguration : IEntityTypeConfiguration<AiAutomationPolicy>
{
    public void Configure(EntityTypeBuilder<AiAutomationPolicy> builder)
    {
        builder.ToTable("AiAutomationPolicies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.TriggerName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.ActionName).HasMaxLength(120).IsRequired();
    }
}
