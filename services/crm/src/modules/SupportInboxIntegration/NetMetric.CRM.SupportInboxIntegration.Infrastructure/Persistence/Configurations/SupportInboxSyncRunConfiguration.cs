// <copyright file="SupportInboxSyncRunConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence.Configurations;

public sealed class SupportInboxSyncRunConfiguration : IEntityTypeConfiguration<SupportInboxSyncRun>
{
    public void Configure(EntityTypeBuilder<SupportInboxSyncRun> builder)
    {
        builder.ToTable("SupportInboxSyncRuns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
    }
}
