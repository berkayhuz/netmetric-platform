// <copyright file="AutomationRuleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class AutomationRuleConfiguration : IEntityTypeConfiguration<AutomationRule>
{
    public void Configure(EntityTypeBuilder<AutomationRule> builder)
    {
        builder.ToTable("AutomationRules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.TriggerType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TriggerDefinitionJson).IsRequired();
        builder.Property(x => x.ConditionDefinitionJson).IsRequired();
        builder.Property(x => x.ActionDefinitionJson).IsRequired();
        builder.Property(x => x.ErrorPolicy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.TemplateKey).HasMaxLength(100);
        builder.Property(x => x.ScheduleCron).HasMaxLength(200);
        builder.Property(x => x.LastExecutionStatus).HasMaxLength(100);
        builder.Property(x => x.ActivationChangedBy).HasMaxLength(200);

        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.TriggerType, x.EntityType, x.IsActive });
        builder.HasIndex(x => new { x.TenantId, x.NextRunAtUtc });

        builder.HasMany(x => x.Versions)
            .WithOne(x => x.Rule)
            .HasForeignKey(x => x.RuleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Versions).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
