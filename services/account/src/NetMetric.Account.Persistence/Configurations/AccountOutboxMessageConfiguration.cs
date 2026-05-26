// <copyright file="AccountOutboxMessageConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.Account.Domain.Outbox;

namespace NetMetric.Account.Persistence.Configurations;

public sealed class AccountOutboxMessageConfiguration : IEntityTypeConfiguration<AccountOutboxMessage>
{
    public void Configure(EntityTypeBuilder<AccountOutboxMessage> builder)
    {
        builder.ToTable("account_outbox_messages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasConversion(StrongIdConversions.TenantId).IsRequired();
        builder.Property(x => x.Type).HasMaxLength(256).IsRequired();
        builder.Property(x => x.PayloadJson).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.OccurredAt).IsRequired();
        builder.Property(x => x.DeadLetteredAt);
        builder.Property(x => x.CorrelationId).HasMaxLength(128);
        builder.Property(x => x.LastError).HasMaxLength(1024);
        builder.Property(x => x.Version).IsRowVersion();

        builder.HasIndex(x => new { x.ProcessedAt, x.NextAttemptAt, x.OccurredAt });
        builder.HasIndex(x => new { x.DeadLetteredAt, x.OccurredAt });
        builder.HasIndex(x => new { x.TenantId, x.Type, x.OccurredAt });
    }
}
