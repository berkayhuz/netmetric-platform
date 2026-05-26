// <copyright file="CustomFieldValueConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomFieldValueConfiguration : IEntityTypeConfiguration<CustomFieldValue>
{
    public void Configure(EntityTypeBuilder<CustomFieldValue> builder)
    {
        builder.ToTable("CustomFieldValues");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityName).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(4000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Definition)
            .WithMany()
            .HasForeignKey(x => x.DefinitionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.DefinitionId, x.EntityName, x.EntityId }).IsUnique();
    }
}
