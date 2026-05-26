// <copyright file="WebhookDeliveryLogConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence.Configurations;

public sealed class WebhookDeliveryLogConfiguration : IEntityTypeConfiguration<WebhookDeliveryLog>
{
    public void Configure(EntityTypeBuilder<WebhookDeliveryLog> builder)
    {
        builder.ToTable("WebhookDeliveryLogs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventKey).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TargetUrl).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RequestPayloadJson).IsRequired();
        builder.Property(x => x.ResponseSnippet).HasMaxLength(1000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.SignatureHeader).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.ExecutionLogId });
        builder.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
    }
}
