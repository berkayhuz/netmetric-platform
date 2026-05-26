// <copyright file="KnowledgeBaseCategoryConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

namespace NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeBaseCategoryConfiguration : IEntityTypeConfiguration<KnowledgeBaseCategory>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseCategory> builder)
    {
        builder.ToTable("KnowledgeBaseCategories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
