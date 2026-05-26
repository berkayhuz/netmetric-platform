// <copyright file="ApprovalWorkflowConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalWorkflows;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class ApprovalWorkflowConfiguration : IEntityTypeConfiguration<ApprovalWorkflow>
{
    public void Configure(EntityTypeBuilder<ApprovalWorkflow> builder)
    {
        builder.ToTable("ApprovalWorkflows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RoutingPolicyJson).IsRequired();
        builder.Property(x => x.EscalationPolicyJson).IsRequired();
        builder.Property(x => x.SlaPolicyJson).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.RelatedEntityId, x.Status });
        builder.Navigation(x => x.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
