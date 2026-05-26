// <copyright file="ApprovalStepConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.ApprovalSteps;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ApproverSelectorJson).IsRequired();
        builder.Property(x => x.EscalationTargetJson).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.WorkflowId, x.Sequence }).IsUnique();
        builder.HasOne(x => x.Workflow)
            .WithMany(x => x.Steps)
            .HasForeignKey(x => x.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
