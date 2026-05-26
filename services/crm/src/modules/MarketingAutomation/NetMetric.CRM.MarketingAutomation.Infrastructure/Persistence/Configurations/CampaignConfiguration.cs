// <copyright file="CampaignConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence.Configurations;

public sealed class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("Campaign");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Channel).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ApprovalStatus).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UtmCampaign).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BudgetAmount).HasPrecision(18, 2);
        builder.Property(x => x.ExpectedRevenueAmount).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Status, x.ScheduledAtUtc });
    }
}
