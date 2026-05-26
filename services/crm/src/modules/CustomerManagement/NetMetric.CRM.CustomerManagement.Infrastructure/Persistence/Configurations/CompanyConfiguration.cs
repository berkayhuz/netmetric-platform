// <copyright file="CompanyConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;


namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.Phone).HasMaxLength(50);
        builder.Property(x => x.Website).HasMaxLength(256);
        builder.Property(x => x.TaxNumber).HasMaxLength(64);
        builder.Property(x => x.TaxOffice).HasMaxLength(128);
        builder.Property(x => x.Sector).HasMaxLength(128);
        builder.Property(x => x.EmployeeCountRange).HasMaxLength(64);
        builder.Property(x => x.AnnualRevenue).HasPrecision(18, 2);
        builder.Property(x => x.LogoStorageKey).HasMaxLength(512);
        builder.Property(x => x.LogoUrl).HasMaxLength(2048);
        builder.Property(x => x.LogoContentType).HasMaxLength(128);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.Name })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.TaxNumber })
            .HasFilter("[TaxNumber] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasOne(x => x.ParentCompany)
            .WithMany(x => x.ChildCompanies)
            .HasForeignKey(x => x.ParentCompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
