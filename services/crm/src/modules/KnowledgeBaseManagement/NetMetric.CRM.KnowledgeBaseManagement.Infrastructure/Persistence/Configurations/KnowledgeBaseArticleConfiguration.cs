// <copyright file="KnowledgeBaseArticleConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.KnowledgeBaseManagement.Domain.Entities;

namespace NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Persistence.Configurations;

public sealed class KnowledgeBaseArticleConfiguration : IEntityTypeConfiguration<KnowledgeBaseArticle>
{
    public void Configure(EntityTypeBuilder<KnowledgeBaseArticle> builder)
    {
        builder.ToTable("KnowledgeBaseArticles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Summary).HasMaxLength(1000);
        builder.Property(x => x.Content).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Slug }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
