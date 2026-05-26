// <copyright file="SupportInboxRuleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence.Configurations;

public sealed class SupportInboxRuleConfiguration : IEntityTypeConfiguration<SupportInboxRule>
{
    public void Configure(EntityTypeBuilder<SupportInboxRule> builder)
    {
        builder.ToTable("SupportInboxRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
        builder.Property(x => x.MatchSender).HasMaxLength(320);
        builder.Property(x => x.MatchSubjectContains).HasMaxLength(250);
    }
}
