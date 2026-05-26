// <copyright file="CampaignMemberConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.CampaignMembers;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence.Configurations;

public sealed class CampaignMemberConfiguration : IEntityTypeConfiguration<CampaignMember>
{
    public void Configure(EntityTypeBuilder<CampaignMember> builder)
    {
        builder.ToTable("CampaignMember");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.EmailAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.ConsentStatus).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CampaignId, x.EmailHash }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
