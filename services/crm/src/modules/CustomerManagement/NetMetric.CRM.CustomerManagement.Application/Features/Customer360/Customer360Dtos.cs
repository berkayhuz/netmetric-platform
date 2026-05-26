// <copyright file="Customer360Dtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Customer360;

public sealed record Customer360Dto(
    Customer360BasicInfoDto BasicInfo,
    Customer360CompanyInfoDto? Company,
    IReadOnlyList<Customer360ContactDto> Contacts,
    IReadOnlyList<Customer360AddressDto> Addresses,
    CustomerLifecycleStage? LifecycleStage,
    Guid? OwnerUserId,
    Customer360TerritoryDto? Territory,
    CustomerConsentSummaryDto ConsentSummary,
    CustomerDataQualityDto? DataQuality,
    CustomerRelationshipHealthDto? RelationshipHealth,
    IReadOnlyList<Customer360SummaryItemDto> OpenLeads,
    IReadOnlyList<Customer360SummaryItemDto> OpenOpportunities,
    IReadOnlyList<Customer360SummaryItemDto> OpenDeals,
    IReadOnlyList<Customer360SummaryItemDto> Quotes,
    IReadOnlyList<Customer360SummaryItemDto> Tickets,
    CustomerFinanceSummaryDto Finance,
    IReadOnlyList<Customer360SummaryItemDto> Contracts,
    IReadOnlyList<Customer360SummaryItemDto> Documents,
    IReadOnlyList<Customer360SummaryItemDto> Activities,
    IReadOnlyList<Customer360SummaryItemDto> Communications,
    IReadOnlyList<Customer360TimelineItemDto> Timeline,
    CustomerPortalLinksDto PortalLinks,
    CustomerAuditSummaryDto AuditSummary,
    IReadOnlyList<CustomerDuplicateWarningDto> DuplicateWarnings,
    IReadOnlyList<Customer360NextActionDto> SuggestedNextActions,
    CustomerAccountHierarchyDto Hierarchy,
    IReadOnlyList<CustomerStakeholderMapItemDto> StakeholderMap,
    CustomerEnrichmentProfileDto? Enrichment);

public sealed record Customer360BasicInfoDto(Guid Id, string FullName, string? Title, string? Email, string? MobilePhone, string? WorkPhone, string? IdentityNumber, bool IsVip, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
public sealed record Customer360CompanyInfoDto(Guid Id, string Name, string? Website, string? Email, string? Phone, string? TaxNumber, string? Sector, Guid? ParentCompanyId);
public sealed record Customer360ContactDto(Guid Id, string FullName, string? Email, string? MobilePhone, bool IsPrimary, bool IsActive);
public sealed record Customer360AddressDto(Guid Id, string AddressType, string Line1, string? City, string? Country, bool IsDefault);
public sealed record Customer360TerritoryDto(Guid Id, string Name, string? Country, string? Region, string? Industry, string? Segment);
public sealed record CustomerConsentSummaryDto(bool MarketingAllowed, IReadOnlyList<CustomerConsentChannelStatusDto> Channels);
public sealed record CustomerConsentChannelStatusDto(CustomerConsentChannel Channel, CustomerConsentPurpose Purpose, CustomerConsentStatus Status, DateTime? ValidUntilUtc);
public sealed record CustomerDataQualityDto(int Score, decimal DuplicateRiskScore, decimal StaleDataRiskScore, IReadOnlyList<string> MissingFields, IReadOnlyList<string> InvalidFields, IReadOnlyList<string> Recommendations, DateTime CalculatedAtUtc);
public sealed record CustomerRelationshipHealthDto(int Score, CustomerRelationshipRiskLevel RiskLevel, DateTime? LastActivityAtUtc, int OpenTicketCount, int OverdueTicketCount, int OpenOpportunityCount, int UnpaidInvoiceCount, DateTime CalculatedAtUtc);
public sealed record Customer360SummaryItemDto(Guid Id, string Title, string? Status, DateTime? OccurredAtUtc, decimal? Amount = null, string? Url = null);
public sealed record CustomerFinanceSummaryDto(decimal? TotalRevenue, decimal? OutstandingBalance, int UnpaidInvoiceCount, int OverdueInvoiceCount, IReadOnlyList<Customer360SummaryItemDto> Invoices, IReadOnlyList<Customer360SummaryItemDto> Payments);
public sealed record Customer360TimelineItemDto(DateTime OccurredAtUtc, string Source, string Action, string Title, Guid? RelatedEntityId, string? Metadata);
public sealed record CustomerPortalLinksDto(string? TicketsUrl, string? ContractsUrl, string? InvoicesUrl, string? DocumentsUrl, string? ProfileUrl, string? SupportUrl);
public sealed record CustomerAuditSummaryDto(int EventCount, DateTime? LastChangedAtUtc, Guid? LastActorUserId);
public sealed record CustomerDuplicateWarningDto(Guid CandidateId, CustomerEntityType EntityType, decimal Score, IReadOnlyList<string> Reasons);
public sealed record Customer360NextActionDto(string Code, string Title, string Priority);
public sealed record CustomerEnrichmentProfileDto(string? Website, string? Domain, string? LinkedInUrl, string? Industry, int? EmployeeCount, decimal? AnnualRevenue, string? Country, string? City, string? Source, decimal ConfidenceScore, DateTime? LastEnrichedAtUtc);
public sealed record CustomerAccountHierarchyDto(IReadOnlyList<CustomerAccountHierarchyNodeDto> Roots);
public sealed record CustomerAccountHierarchyNodeDto(Guid Id, Guid CompanyId, Guid? ParentCompanyId, string Name, string RelationshipType, int DisplayOrder, bool IsPrimary, IReadOnlyList<CustomerAccountHierarchyNodeDto> Children);
public sealed record CustomerStakeholderMapItemDto(Guid Id, Guid CompanyId, Guid ContactId, string ContactName, CustomerStakeholderRole Role, CustomerInfluenceLevel InfluenceLevel, CustomerSentiment Sentiment, bool IsPrimary);
