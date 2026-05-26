// <copyright file="RuleExecutionLogConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class RuleExecutionLogConfiguration : IEntityTypeConfiguration<RuleExecutionLog>
{
    public void Configure(EntityTypeBuilder<RuleExecutionLog> builder)
    {
        builder.ToTable("RuleExecutionLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RuleName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TriggerType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LoopFingerprint).HasMaxLength(500).IsRequired();
        builder.Property(x => x.LockedBy).HasMaxLength(200);
        builder.Property(x => x.ErrorClassification).HasMaxLength(100);
        builder.Property(x => x.ErrorCode).HasMaxLength(100);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.TriggerPayloadJson).IsRequired();
        builder.Property(x => x.ConditionResultJson).IsRequired();
        builder.Property(x => x.ActionResultJson).IsRequired();
        builder.Property(x => x.PermissionSnapshotJson).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.RuleId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.LoopFingerprint, x.StartedAtUtc });
    }
}
