// <copyright file="ContractDocumentConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.ContractDocuments;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence.Configurations;

public sealed class ContractDocumentConfiguration : IEntityTypeConfiguration<ContractDocument>
{
    public void Configure(EntityTypeBuilder<ContractDocument> builder)
    {
        builder.ToTable("ContractDocument");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.HasIndex(x => new { x.TenantId, x.Code }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
