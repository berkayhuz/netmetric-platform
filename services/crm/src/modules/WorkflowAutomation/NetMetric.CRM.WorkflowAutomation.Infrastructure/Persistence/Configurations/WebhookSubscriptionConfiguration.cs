// <copyright file="WebhookSubscriptionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("WebhookSubscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EventKey).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TargetUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.SecretKeyReference).HasMaxLength(500).IsRequired();
        builder.Property(x => x.SigningAlgorithm).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.EventKey, x.IsActive });
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}
