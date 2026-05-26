// <copyright file="CustomerOperationalEntities.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;

public enum CustomerEntityType { Customer = 0, Company = 1, Contact = 2 }
public enum CustomerConsentChannel { Email = 0, Sms = 1, Phone = 2, WhatsApp = 3, Push = 4 }
public enum CustomerConsentPurpose { Marketing = 0, Transactional = 1, Support = 2, ProductUpdates = 3 }
public enum CustomerConsentStatus { Unknown = 0, OptedIn = 1, OptedOut = 2, Revoked = 3 }
public enum CustomerConsentSource { Manual = 0, Import = 1, Form = 2, Api = 3, Portal = 4 }
public enum CustomerLifecycleStage { Lead = 0, Prospect = 1, ActiveCustomer = 2, ChurnRisk = 3, Churned = 4, Reactivated = 5 }
public enum CustomerRelationshipType { Parent = 0, Subsidiary = 1, Division = 2, Department = 3, Partner = 4, Reseller = 5, Vendor = 6 }
public enum CustomerStakeholderRole { DecisionMaker = 0, EconomicBuyer = 1, TechnicalBuyer = 2, Influencer = 3, Champion = 4, Blocker = 5, Legal = 6, Finance = 7, Sponsor = 8, EndUser = 9 }
public enum CustomerInfluenceLevel { Low = 0, Medium = 1, High = 2 }
public enum CustomerSentiment { Positive = 0, Neutral = 1, Negative = 2, Unknown = 3 }
public enum CustomerDuplicateStatus { Open = 0, Ignored = 1, Merged = 2, FalsePositive = 3 }
public enum CustomerImportBatchStatus { Uploaded = 0, Mapped = 1, Validated = 2, Importing = 3, Completed = 4, Failed = 5, Cancelled = 6 }
public enum CustomerImportRowStatus { Pending = 0, Valid = 1, Invalid = 2, Duplicate = 3, Imported = 4, Skipped = 5 }
public enum CustomerDuplicateStrategy { Skip = 0, UpdateExisting = 1, CreateNew = 2, Merge = 3 }
public enum CustomerRecordAccessLevel { Read = 0, Comment = 1, Edit = 2, OwnerDelegate = 3 }
public enum CustomerAuditAction { Created = 0, Updated = 1, Deleted = 2, Merged = 3, OwnerChanged = 4, StageChanged = 5, ConsentChanged = 6, Shared = 7, Imported = 8, Enriched = 9, DataQualityCalculated = 10, RelationshipHealthCalculated = 11 }
public enum CustomerRelationshipRiskLevel { Low = 0, Medium = 1, High = 2, Critical = 3 }

public sealed class CustomerStakeholder : AuditableEntity
{
    public Guid CompanyId { get; set; }
    public Guid ContactId { get; set; }
    public Guid? RelatedOpportunityId { get; set; }
    public CustomerStakeholderRole Role { get; set; }
    public CustomerInfluenceLevel InfluenceLevel { get; set; } = CustomerInfluenceLevel.Medium;
    public CustomerSentiment Sentiment { get; set; } = CustomerSentiment.Unknown;
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class CustomerConsent : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public CustomerConsentChannel Channel { get; set; }
    public CustomerConsentPurpose Purpose { get; set; }
    public CustomerConsentStatus Status { get; set; } = CustomerConsentStatus.Unknown;
    public CustomerConsentSource Source { get; set; } = CustomerConsentSource.Manual;
    public DateTime ValidFromUtc { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public string? EvidenceText { get; set; }
    public string? EvidenceIpAddress { get; set; }
    public string? EvidenceUserAgent { get; set; }
    public ICollection<CustomerConsentHistory> History { get; set; } = [];
}

public sealed class CustomerConsentHistory : AuditableEntity
{
    public Guid ConsentId { get; set; }
    public CustomerConsent? Consent { get; set; }
    public CustomerConsentStatus PreviousStatus { get; set; }
    public CustomerConsentStatus NewStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
    public CustomerConsentSource Source { get; set; }
}

public sealed class CustomerLifecycleStageHistory : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public CustomerLifecycleStage? PreviousStage { get; set; }
    public CustomerLifecycleStage NewStage { get; set; }
    public DateTime ChangedAtUtc { get; set; }
    public string? ChangedBy { get; set; }
    public string? Reason { get; set; }
}

public sealed class CustomerEnrichmentProfile : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string? Website { get; set; }
    public string? Domain { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? Industry { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? SocialProfilesJson { get; set; }
    public string? Source { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTime? LastEnrichedAtUtc { get; set; }
    public string? RawDataJson { get; set; }
}

public sealed class CustomerDataQualitySnapshot : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public int Score { get; set; }
    public string? MissingFieldsJson { get; set; }
    public string? InvalidFieldsJson { get; set; }
    public decimal DuplicateRiskScore { get; set; }
    public decimal StaleDataRiskScore { get; set; }
    public string? RecommendationsJson { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
}

public sealed class CustomerTerritory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Country { get; set; }
    public string? Region { get; set; }
    public string? Industry { get; set; }
    public string? Segment { get; set; }
}

public sealed class CustomerOwnershipRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string ConditionJson { get; set; } = "{}";
    public Guid AssignedOwnerId { get; set; }
    public Guid? AssignedTeamId { get; set; }
}

public sealed class CustomerOwnershipAssignmentHistory : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public Guid? PreviousOwnerId { get; set; }
    public Guid NewOwnerId { get; set; }
    public Guid? RuleId { get; set; }
    public DateTime AssignedAtUtc { get; set; }
    public string? AssignedBy { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class CustomerImportBatch : AuditableEntity
{
    public string FileName { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public CustomerImportBatchStatus Status { get; set; } = CustomerImportBatchStatus.Uploaded;
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
    public DateTime? CompletedAt { get; set; }
    public ICollection<CustomerImportRow> Rows { get; set; } = [];
}

public sealed class CustomerImportRow : AuditableEntity
{
    public Guid BatchId { get; set; }
    public CustomerImportBatch? Batch { get; set; }
    public int RowNumber { get; set; }
    public string RawDataJson { get; set; } = "{}";
    public string? MappedDataJson { get; set; }
    public string? ValidationErrorsJson { get; set; }
    public string? DuplicateWarningsJson { get; set; }
    public CustomerImportRowStatus Status { get; set; } = CustomerImportRowStatus.Pending;
    public Guid? ImportedEntityId { get; set; }
}

public sealed class CustomerRecordShare : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public Guid? SharedWithUserId { get; set; }
    public Guid? SharedWithTeamId { get; set; }
    public CustomerRecordAccessLevel AccessLevel { get; set; }
    public DateTime? ValidUntilUtc { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class CustomerAuditEvent : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public CustomerAuditAction Action { get; set; }
    public string? FieldName { get; set; }
    public string? OldValueMasked { get; set; }
    public string? NewValueMasked { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? MetadataJson { get; set; }
}

public sealed class CustomerSearchDocument : AuditableEntity
{
    public CustomerEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? NormalizedEmail { get; set; }
    public string? NormalizedPhone { get; set; }
    public string? CompanyName { get; set; }
    public string? Domain { get; set; }
    public string? Tags { get; set; }
    public string SearchText { get; set; } = string.Empty;
    public DateTime LastIndexedAtUtc { get; set; }
}

public sealed class CustomerRelationshipHealthSnapshot : AuditableEntity
{
    public Guid CustomerId { get; set; }
    public int Score { get; set; }
    public DateTime? LastActivityAtUtc { get; set; }
    public int OpenTicketCount { get; set; }
    public int OverdueTicketCount { get; set; }
    public int OpenOpportunityCount { get; set; }
    public int WonDealCount { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public int? RenewalDueInDays { get; set; }
    public CustomerRelationshipRiskLevel RiskLevel { get; set; }
    public string? SignalsJson { get; set; }
    public DateTime CalculatedAtUtc { get; set; }
}
