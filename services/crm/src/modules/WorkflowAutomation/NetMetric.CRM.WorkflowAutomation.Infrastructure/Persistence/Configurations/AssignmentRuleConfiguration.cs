// <copyright file="AssignmentRuleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AssignmentRules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class AssignmentRuleConfiguration : IEntityTypeConfiguration<AssignmentRule>
{
    public void Configure(EntityTypeBuilder<AssignmentRule> builder)
    {
        builder.ToTable("AssignmentRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConditionJson).IsRequired();
        builder.Property(x => x.AssigneeSelectorJson).IsRequired();
        builder.Property(x => x.FallbackAssigneeJson).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.Priority });
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}
