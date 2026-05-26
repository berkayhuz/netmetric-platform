// <copyright file="EmailCampaignConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.EmailCampaigns;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence.Configurations;

public sealed class EmailCampaignConfiguration : IEntityTypeConfiguration<EmailCampaign>
{
    public void Configure(EntityTypeBuilder<EmailCampaign> builder)
    {
        builder.ToTable("EmailCampaign");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Subject).HasMaxLength(300).IsRequired();
        builder.Property(x => x.FromName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FromEmail).HasMaxLength(320).IsRequired();
        builder.Property(x => x.HtmlBody).IsRequired();
        builder.Property(x => x.TextBody).IsRequired();
        builder.Property(x => x.DeliverabilityStatus).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
