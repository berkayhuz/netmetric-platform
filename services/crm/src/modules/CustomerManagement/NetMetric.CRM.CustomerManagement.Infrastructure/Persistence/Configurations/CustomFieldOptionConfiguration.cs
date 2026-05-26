// <copyright file="CustomFieldOptionConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomFields;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations;

public sealed class CustomFieldOptionConfiguration : IEntityTypeConfiguration<CustomFieldOption>
{
    public void Configure(EntityTypeBuilder<CustomFieldOption> builder)
    {
        builder.ToTable("CustomFieldOptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Label).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(150).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CustomFieldDefinitionId, x.Value }).IsUnique();
    }
}
