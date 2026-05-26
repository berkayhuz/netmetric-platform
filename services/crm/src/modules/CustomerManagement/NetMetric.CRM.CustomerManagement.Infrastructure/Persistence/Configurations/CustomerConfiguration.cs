// <copyright file="CustomerConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;


namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.MobilePhone).HasMaxLength(50);
        builder.Property(x => x.WorkPhone).HasMaxLength(50);
        builder.Property(x => x.IdentityNumber).HasMaxLength(64);
        builder.Property(x => x.ProfileImageStorageKey).HasMaxLength(512);
        builder.Property(x => x.ProfileImageUrl).HasMaxLength(2048);
        builder.Property(x => x.ProfileImageContentType).HasMaxLength(128);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CompanyId });
        builder.HasIndex(x => new { x.TenantId, x.Email })
            .HasFilter("[Email] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.IdentityNumber })
            .HasFilter("[IdentityNumber] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
