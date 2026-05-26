// <copyright file="OpportunityContactConfiguration.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence.Configurations;

public sealed class OpportunityContactConfiguration : IEntityTypeConfiguration<OpportunityContact>
{
    public void Configure(EntityTypeBuilder<OpportunityContact> builder)
    {
        builder.ToTable("OpportunityContacts");
        builder.HasIndex(x => new { x.TenantId, x.OpportunityId, x.ContactId }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.OpportunityId, x.IsPrimary }).HasFilter("[IsPrimary] = 1");
    }
}
