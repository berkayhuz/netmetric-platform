// <copyright file="Customer360Abstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;

public interface ICustomerLeadSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenLeadsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerOpportunitySummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenOpportunitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerDealSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenDealsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerQuoteSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetQuotesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerTicketSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetTicketsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerFinanceSummaryProvider { Task<CustomerFinanceSummaryDto> GetFinanceSummaryAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerContractSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetContractsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerDocumentSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetDocumentsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerActivitySummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetActivitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerCommunicationSummaryProvider { Task<IReadOnlyList<Customer360SummaryItemDto>> GetCommunicationsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerAccountHierarchyProvider { Task<CustomerAccountHierarchyDto> GetHierarchyAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }
public interface ICustomerStakeholderMapProvider { Task<IReadOnlyList<CustomerStakeholderMapItemDto>> GetStakeholdersAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken); }

public interface IDuplicateDetectionService
{
    string? NormalizeEmail(string? email);
    string? NormalizePhone(string? phone);
    string? NormalizeDomain(string? websiteOrDomain);
    decimal CalculateNameSimilarity(string? left, string? right);
    Task<IReadOnlyList<CustomerDuplicateWarningDto>> FindCustomerDuplicatesAsync(Customer customer, CancellationToken cancellationToken);
}

public interface ICustomerDataQualityService
{
    CustomerDataQualitySnapshot Calculate(Customer customer, IReadOnlyCollection<CustomerConsent> consents, decimal duplicateRiskScore, DateTime? lastActivityAtUtc);
}

public interface ICustomerRelationshipHealthService
{
    CustomerRelationshipHealthSnapshot Calculate(Customer customer, DateTime? lastActivityAtUtc, int openTickets, int overdueTickets, int openOpportunities, int wonDeals, int unpaidInvoices, int? renewalDueInDays, CustomerLifecycleStage? lifecycleStage);
}

public interface ICustomerAuditTrailWriter
{
    Task WriteAsync(CustomerEntityType entityType, Guid entityId, CustomerAuditAction action, string? fieldName, string? oldValue, string? newValue, string? metadataJson, CancellationToken cancellationToken);
}

public interface ICustomerOwnershipRuleEvaluator
{
    Task<CustomerOwnershipRule?> EvaluateAsync(Customer customer, Company? company, CancellationToken cancellationToken);
}

public interface ICustomerSearchIndexer
{
    Task<CustomerSearchDocument> ReindexCustomerAsync(Guid customerId, CancellationToken cancellationToken);
}

public interface ICustomerSearchService
{
    Task<IReadOnlyList<Customer360SummaryItemDto>> SearchAsync(string term, int take, CancellationToken cancellationToken);
}

public interface ICustomerEnrichmentProvider
{
    Task<CustomerEnrichmentProfile?> EnrichAsync(CustomerEntityType entityType, Guid entityId, CancellationToken cancellationToken);
}

public interface ICustomerEnrichmentService
{
    Task<CustomerEnrichmentProfile> UpsertManualAsync(CustomerEnrichmentProfile profile, CancellationToken cancellationToken);
}
