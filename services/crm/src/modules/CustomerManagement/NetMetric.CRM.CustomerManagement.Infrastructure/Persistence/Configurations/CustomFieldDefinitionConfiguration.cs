// <copyright file="CustomFieldDefinitionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomFieldDefinitionConfiguration : IEntityTypeConfiguration<CustomFieldDefinition>
{
    public void Configure(EntityTypeBuilder<CustomFieldDefinition> builder)
    {
        builder.ToTable("CustomFieldDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(150).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.DefaultValue).HasMaxLength(1000);
        builder.Property(x => x.Placeholder).HasMaxLength(250);
        builder.Property(x => x.HelpText).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasMany(x => x.Options)
            .WithOne(x => x.CustomFieldDefinition)
            .HasForeignKey(x => x.CustomFieldDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.EntityName, x.Name }).IsUnique();
    }
}
