// <copyright file="DocumentConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Documents;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.OriginalFileName).HasMaxLength(256).IsRequired();
        builder.Property(x => x.FileExtension).HasMaxLength(32).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(x => x.PathOrUrl).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.CustomerId, x.ContactId });
    }
}
