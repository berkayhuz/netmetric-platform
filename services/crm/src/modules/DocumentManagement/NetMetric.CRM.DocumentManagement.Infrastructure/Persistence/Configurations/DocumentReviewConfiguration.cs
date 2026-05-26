// <copyright file="DocumentReviewConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentReviews;

namespace NetMetric.CRM.DocumentManagement.Infrastructure.Persistence.Configurations;

public sealed class DocumentReviewConfiguration : IEntityTypeConfiguration<DocumentReview>
{
    public void Configure(EntityTypeBuilder<DocumentReview> builder)
    {
        builder.ToTable("DocumentReviews");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.OccurredAtUtc).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name });
    }
}
