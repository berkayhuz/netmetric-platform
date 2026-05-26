// <copyright file="GuidedSellingPlaybookConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class GuidedSellingPlaybookConfiguration : IEntityTypeConfiguration<GuidedSellingPlaybook>
{
    public void Configure(EntityTypeBuilder<GuidedSellingPlaybook> builder)
    {
        builder.ToTable("GuidedSellingPlaybooks", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Segment).HasMaxLength(128);
        builder.Property(x => x.Industry).HasMaxLength(128);
        builder.Property(x => x.RequiredCapabilities).HasMaxLength(1024);
        builder.Property(x => x.RecommendedBundleCodes).HasMaxLength(1024).IsRequired();
        builder.Property(x => x.MinimumBudget).HasPrecision(18, 2);
        builder.Property(x => x.MaximumBudget).HasPrecision(18, 2);
        builder.Property(x => x.QualificationJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
