// <copyright file="ProposalTemplateConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.QuoteManagement.Domain.Entities;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence.Configurations;

public sealed class ProposalTemplateConfiguration : IEntityTypeConfiguration<ProposalTemplate>
{
    public void Configure(EntityTypeBuilder<ProposalTemplate> builder)
    {
        builder.ToTable("ProposalTemplates", "quote");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(128).IsRequired();
        builder.Property(x => x.SubjectTemplate).HasMaxLength(256);
        builder.Property(x => x.BodyTemplate).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}
