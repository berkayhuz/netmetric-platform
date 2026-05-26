// <copyright file="CustomerManagementOutboxMessageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomerManagementOutboxMessageConfiguration : IEntityTypeConfiguration<CustomerManagementOutboxMessage>
{
    public void Configure(EntityTypeBuilder<CustomerManagementOutboxMessage> builder)
    {
        builder.ToTable("CustomerManagementOutboxMessages");
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
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique()
            .HasFilter("[IdempotencyKey] IS NOT NULL");
    }
}
