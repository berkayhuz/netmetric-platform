// <copyright file="AutomationReminderConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationReminders;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class AutomationReminderConfiguration : IEntityTypeConfiguration<AutomationReminder>
{
    public void Configure(EntityTypeBuilder<AutomationReminder> builder)
    {
        builder.ToTable("AutomationReminders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ReminderType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RecipientSelectorJson).IsRequired();
        builder.Property(x => x.PayloadJson).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.DueAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.RelatedEntityId });
    }
}
