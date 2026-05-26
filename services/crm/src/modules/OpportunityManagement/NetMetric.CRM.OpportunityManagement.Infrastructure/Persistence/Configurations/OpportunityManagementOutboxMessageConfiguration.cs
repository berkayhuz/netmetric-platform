// <copyright file="OpportunityManagementOutboxMessageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;
namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;
public sealed class OpportunityManagementOutboxMessageConfiguration : IEntityTypeConfiguration<OpportunityManagementOutboxMessage>
{
    public void Configure(EntityTypeBuilder<OpportunityManagementOutboxMessage> builder)
    {
        builder.ToTable("OpportunityManagementOutboxMessages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.RoutingKey).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PayloadJson).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(160);
        builder.Property(x => x.LockedBy).HasMaxLength(128);
        builder.Property(x => x.LastError).HasMaxLength(1024);
        builder.Property(x => x.Version).IsRowVersion();
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.DeadLetteredAtUtc, x.NextAttemptAtUtc, x.LockedUntilUtc, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EventName, x.OccurredAtUtc });
        builder.HasIndex(x => x.IdempotencyKey).IsUnique().HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
