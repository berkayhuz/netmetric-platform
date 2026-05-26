// <copyright file="OpportunityStageHistoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class OpportunityStageHistoryConfiguration : IEntityTypeConfiguration<OpportunityStageHistory>
{
    public void Configure(EntityTypeBuilder<OpportunityStageHistory> builder)
    {
        builder.ToTable("OpportunityStageHistories");
        builder.Property(x => x.Note).HasMaxLength(1000);
    }
}
