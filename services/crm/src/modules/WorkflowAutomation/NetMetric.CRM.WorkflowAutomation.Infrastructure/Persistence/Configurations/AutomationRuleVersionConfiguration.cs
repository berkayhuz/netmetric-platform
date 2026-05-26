// <copyright file="AutomationRuleVersionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleVersionConfiguration : IEntityTypeConfiguration<AutomationRuleVersion>
{
    public void Configure(EntityTypeBuilder<AutomationRuleVersion> builder)
    {
        builder.ToTable("AutomationRuleVersions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TriggerDefinitionJson).IsRequired();
        builder.Property(x => x.ConditionDefinitionJson).IsRequired();
        builder.Property(x => x.ActionDefinitionJson).IsRequired();
        builder.Property(x => x.ChangeReason).HasMaxLength(200).IsRequired();
        builder.Property(x => x.ChangedBy).HasMaxLength(200);
        builder.HasIndex(x => new { x.TenantId, x.RuleId, x.Version }).IsUnique();
    }
}
