// <copyright file="CustomerOperationalEntityConfigurations.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Configurations.CustomerOperations;

public sealed class CustomerStakeholderConfiguration : IEntityTypeConfiguration<CustomerStakeholder>
{
    public void Configure(EntityTypeBuilder<CustomerStakeholder> builder)
    {
        builder.ToTable("CustomerStakeholders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CompanyId, x.ContactId, x.Role }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CompanyId });
        builder.HasIndex(x => new { x.TenantId, x.ContactId });
    }
}

public sealed class CustomerConsentConfiguration : IEntityTypeConfiguration<CustomerConsent>
{
    public void Configure(EntityTypeBuilder<CustomerConsent> builder)
    {
        builder.ToTable("CustomerConsents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EvidenceText).HasMaxLength(2000);
        builder.Property(x => x.EvidenceIpAddress).HasMaxLength(128);
        builder.Property(x => x.EvidenceUserAgent).HasMaxLength(512);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.Channel, x.Purpose }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.EntityId });
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}

public sealed class CustomerConsentHistoryConfiguration : IEntityTypeConfiguration<CustomerConsentHistory>
{
    public void Configure(EntityTypeBuilder<CustomerConsentHistory> builder)
    {
        builder.ToTable("CustomerConsentHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChangedBy).HasMaxLength(256);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Consent).WithMany(x => x.History).HasForeignKey(x => x.ConsentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(x => new { x.TenantId, x.ConsentId, x.ChangedAtUtc });
    }
}

public sealed class CustomerLifecycleStageHistoryConfiguration : IEntityTypeConfiguration<CustomerLifecycleStageHistory>
{
    public void Configure(EntityTypeBuilder<CustomerLifecycleStageHistory> builder)
    {
        builder.ToTable("CustomerLifecycleStageHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ChangedBy).HasMaxLength(256);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CustomerId, x.ChangedAtUtc });
    }
}

public sealed class CustomerEnrichmentProfileConfiguration : IEntityTypeConfiguration<CustomerEnrichmentProfile>
{
    public void Configure(EntityTypeBuilder<CustomerEnrichmentProfile> builder)
    {
        builder.ToTable("CustomerEnrichmentProfiles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Website).HasMaxLength(512);
        builder.Property(x => x.Domain).HasMaxLength(256);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(512);
        builder.Property(x => x.Industry).HasMaxLength(128);
        builder.Property(x => x.Country).HasMaxLength(128);
        builder.Property(x => x.City).HasMaxLength(128);
        builder.Property(x => x.Source).HasMaxLength(128);
        builder.Property(x => x.SocialProfilesJson).HasMaxLength(4000);
        builder.Property(x => x.RawDataJson).HasMaxLength(8000);
        builder.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
        builder.Property(x => x.AnnualRevenue).HasPrecision(18, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Domain }).HasFilter("[Domain] IS NOT NULL AND [IsDeleted] = 0");
    }
}

public sealed class CustomerDataQualitySnapshotConfiguration : IEntityTypeConfiguration<CustomerDataQualitySnapshot>
{
    public void Configure(EntityTypeBuilder<CustomerDataQualitySnapshot> builder)
    {
        builder.ToTable("CustomerDataQualitySnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MissingFieldsJson).HasMaxLength(4000);
        builder.Property(x => x.InvalidFieldsJson).HasMaxLength(4000);
        builder.Property(x => x.RecommendationsJson).HasMaxLength(4000);
        builder.Property(x => x.DuplicateRiskScore).HasPrecision(5, 2);
        builder.Property(x => x.StaleDataRiskScore).HasPrecision(5, 2);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CalculatedAtUtc });
    }
}

public sealed class CustomerTerritoryConfiguration : IEntityTypeConfiguration<CustomerTerritory>
{
    public void Configure(EntityTypeBuilder<CustomerTerritory> builder)
    {
        builder.ToTable("CustomerTerritories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Country).HasMaxLength(128);
        builder.Property(x => x.Region).HasMaxLength(128);
        builder.Property(x => x.Industry).HasMaxLength(128);
        builder.Property(x => x.Segment).HasMaxLength(128);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CustomerOwnershipRuleConfiguration : IEntityTypeConfiguration<CustomerOwnershipRule>
{
    public void Configure(EntityTypeBuilder<CustomerOwnershipRule> builder)
    {
        builder.ToTable("CustomerOwnershipRules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ConditionJson).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Priority });
        builder.HasIndex(x => new { x.TenantId, x.Name }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class CustomerOwnershipAssignmentHistoryConfiguration : IEntityTypeConfiguration<CustomerOwnershipAssignmentHistory>
{
    public void Configure(EntityTypeBuilder<CustomerOwnershipAssignmentHistory> builder)
    {
        builder.ToTable("CustomerOwnershipAssignmentHistories");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AssignedBy).HasMaxLength(256);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CustomerId, x.AssignedAtUtc });
    }
}

public sealed class CustomerImportBatchConfiguration : IEntityTypeConfiguration<CustomerImportBatch>
{
    public void Configure(EntityTypeBuilder<CustomerImportBatch> builder)
    {
        builder.ToTable("CustomerImportBatches");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(128).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.CreatedAt });
    }
}

public sealed class CustomerImportRowConfiguration : IEntityTypeConfiguration<CustomerImportRow>
{
    public void Configure(EntityTypeBuilder<CustomerImportRow> builder)
    {
        builder.ToTable("CustomerImportRows");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RawDataJson).HasMaxLength(8000).IsRequired();
        builder.Property(x => x.MappedDataJson).HasMaxLength(8000);
        builder.Property(x => x.ValidationErrorsJson).HasMaxLength(4000);
        builder.Property(x => x.DuplicateWarningsJson).HasMaxLength(4000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Batch).WithMany(x => x.Rows).HasForeignKey(x => x.BatchId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.TenantId, x.BatchId, x.RowNumber }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
    }
}

public sealed class CustomerRecordShareConfiguration : IEntityTypeConfiguration<CustomerRecordShare>
{
    public void Configure(EntityTypeBuilder<CustomerRecordShare> builder)
    {
        builder.ToTable("CustomerRecordShares");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.TenantId, x.SharedWithUserId }).HasFilter("[SharedWithUserId] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.SharedWithTeamId }).HasFilter("[SharedWithTeamId] IS NOT NULL AND [IsDeleted] = 0");
    }
}

public sealed class CustomerAuditEventConfiguration : IEntityTypeConfiguration<CustomerAuditEvent>
{
    public void Configure(EntityTypeBuilder<CustomerAuditEvent> builder)
    {
        builder.ToTable("CustomerAuditEvents");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FieldName).HasMaxLength(128);
        builder.Property(x => x.OldValueMasked).HasMaxLength(2000);
        builder.Property(x => x.NewValueMasked).HasMaxLength(2000);
        builder.Property(x => x.CorrelationId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.MetadataJson).HasMaxLength(4000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.CorrelationId });
    }
}

public sealed class CustomerSearchDocumentConfiguration : IEntityTypeConfiguration<CustomerSearchDocument>
{
    public void Configure(EntityTypeBuilder<CustomerSearchDocument> builder)
    {
        builder.ToTable("CustomerSearchDocuments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Subtitle).HasMaxLength(512);
        builder.Property(x => x.NormalizedEmail).HasMaxLength(256);
        builder.Property(x => x.NormalizedPhone).HasMaxLength(64);
        builder.Property(x => x.CompanyName).HasMaxLength(256);
        builder.Property(x => x.Domain).HasMaxLength(256);
        builder.Property(x => x.Tags).HasMaxLength(1000);
        builder.Property(x => x.SearchText).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.EntityType, x.EntityId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NormalizedEmail }).HasFilter("[NormalizedEmail] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.NormalizedPhone }).HasFilter("[NormalizedPhone] IS NOT NULL AND [IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.Domain }).HasFilter("[Domain] IS NOT NULL AND [IsDeleted] = 0");
    }
}

public sealed class CustomerRelationshipHealthSnapshotConfiguration : IEntityTypeConfiguration<CustomerRelationshipHealthSnapshot>
{
    public void Configure(EntityTypeBuilder<CustomerRelationshipHealthSnapshot> builder)
    {
        builder.ToTable("CustomerRelationshipHealthSnapshots");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SignalsJson).HasMaxLength(4000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.TenantId, x.CustomerId }).IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.TenantId, x.CalculatedAtUtc });
    }
}
