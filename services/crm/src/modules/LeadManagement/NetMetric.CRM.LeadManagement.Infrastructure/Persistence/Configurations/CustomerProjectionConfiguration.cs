// <copyright file="CustomerProjectionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Core;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomerProjectionConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(64);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.Property(x => x.MobilePhone).HasMaxLength(50);
        builder.Property(x => x.WorkPhone).HasMaxLength(50);
        builder.Property(x => x.PersonalPhone).HasMaxLength(50);
        builder.Property(x => x.Department).HasMaxLength(128);
        builder.Property(x => x.JobTitle).HasMaxLength(128);
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.IdentityNumber).HasMaxLength(64);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.Ignore(x => x.Company);
        builder.Ignore(x => x.Addresses);
    }
}
