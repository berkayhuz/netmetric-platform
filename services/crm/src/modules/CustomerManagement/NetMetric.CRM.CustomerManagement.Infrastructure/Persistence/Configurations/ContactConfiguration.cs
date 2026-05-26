// <copyright file="ContactConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;


namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.MobilePhone).HasMaxLength(50);
        builder.Property(x => x.WorkPhone).HasMaxLength(50);
        builder.Property(x => x.PersonalPhone).HasMaxLength(50);
        builder.Property(x => x.Department).HasMaxLength(100);
        builder.Property(x => x.JobTitle).HasMaxLength(100);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");

        builder.HasIndex(x => new { x.TenantId, x.CompanyId })
            .HasFilter("[IsPrimaryContact] = 1 AND [IsDeleted] = 0 AND [CompanyId] IS NOT NULL")
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.CustomerId })
            .HasFilter("[IsPrimaryContact] = 1 AND [IsDeleted] = 0 AND [CustomerId] IS NOT NULL")
            .IsUnique();

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
