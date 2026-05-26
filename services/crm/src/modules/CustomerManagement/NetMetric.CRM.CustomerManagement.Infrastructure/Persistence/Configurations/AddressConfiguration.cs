// <copyright file="AddressConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Line1).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Line2).HasMaxLength(250);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.State).HasMaxLength(100);
        builder.Property(x => x.Country).HasMaxLength(100);
        builder.Property(x => x.ZipCode).HasMaxLength(30);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.TenantId, x.CompanyId })
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0 AND [CompanyId] IS NOT NULL")
            .IsUnique();

        builder.HasIndex(x => new { x.TenantId, x.CustomerId })
            .HasFilter("[IsDefault] = 1 AND [IsDeleted] = 0 AND [CustomerId] IS NOT NULL")
            .IsUnique();

        builder.HasOne(x => x.Company)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
