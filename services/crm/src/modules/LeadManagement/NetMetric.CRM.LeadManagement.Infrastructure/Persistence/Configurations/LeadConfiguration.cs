// <copyright file="LeadConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Persistence.Configurations;

public sealed class LeadConfiguration : IEntityTypeConfiguration<Lead>
{
    public void Configure(EntityTypeBuilder<Lead> builder)
    {
        builder.ToTable("Leads");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.LeadCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(200);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.JobTitle).HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.EstimatedBudget).HasPrecision(18, 2);
        builder.Property(x => x.SourceRoi).HasPrecision(18, 2);

        builder.Property(x => x.UtmSource).HasMaxLength(128);
        builder.Property(x => x.UtmMedium).HasMaxLength(128);
        builder.Property(x => x.UtmCampaign).HasMaxLength(128);
        builder.Property(x => x.UtmTerm).HasMaxLength(256);
        builder.Property(x => x.UtmContent).HasMaxLength(256);
        builder.Property(x => x.ReferrerUrl).HasMaxLength(1024);

        builder.Property(x => x.FitScore).HasPrecision(9, 2);
        builder.Property(x => x.EngagementScore).HasPrecision(9, 2);
        builder.Property(x => x.TotalScore).HasPrecision(9, 2);
        builder.Property(x => x.AiScore).HasPrecision(9, 2);

        builder.Property(x => x.QualificationData).HasColumnType("nvarchar(max)");
        builder.Property(x => x.DisqualificationNote).HasMaxLength(1000);

        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.LeadCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(x => x.DisqualificationReason)
            .WithMany()
            .HasForeignKey(x => x.DisqualificationReasonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ConvertedCustomer)
            .WithMany()
            .HasForeignKey(x => x.ConvertedCustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.MergedIntoLead)
            .WithMany()
            .HasForeignKey(x => x.MergedIntoLeadId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(x => x.Company);
    }
}
