// <copyright file="GetCustomer360Query.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Features.Customer360;

public sealed record GetCustomer360Query(Guid CustomerId) : IRequest<Customer360Dto>;

public sealed class GetCustomer360QueryHandler(
    ICustomerManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ICustomerManagementSecurityService securityService,
    IDuplicateDetectionService duplicateDetectionService,
    ICustomerLeadSummaryProvider leadProvider,
    ICustomerOpportunitySummaryProvider opportunityProvider,
    ICustomerDealSummaryProvider dealProvider,
    ICustomerQuoteSummaryProvider quoteProvider,
    ICustomerTicketSummaryProvider ticketProvider,
    ICustomerFinanceSummaryProvider financeProvider,
    ICustomerContractSummaryProvider contractProvider,
    ICustomerDocumentSummaryProvider documentProvider,
    ICustomerActivitySummaryProvider activityProvider,
    ICustomerCommunicationSummaryProvider communicationProvider,
    ICustomerAccountHierarchyProvider hierarchyProvider,
    ICustomerStakeholderMapProvider stakeholderMapProvider,
    IOptions<CustomerPortalOptions> portalOptions) : IRequestHandler<GetCustomer360Query, Customer360Dto>
{
    public async Task<Customer360Dto> Handle(GetCustomer360Query request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();
        currentUserService.EnsureAuthenticated();

        var customer = await securityService.ApplyCustomerReadScope(dbContext.Customers)
            .AsNoTracking()
            .Include(x => x.Company)
            .Include(x => x.Contacts)
            .Include(x => x.Addresses)
            .Where(x => x.TenantId == tenantId && x.Id == request.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (customer is null)
            throw new NotFoundAppException("Customer not found.");

        var consentTask = dbContext.CustomerConsents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id)
            .ToListAsync(cancellationToken);
        var qualityTask = dbContext.CustomerDataQualitySnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id)
            .OrderByDescending(x => x.CalculatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var healthTask = dbContext.CustomerRelationshipHealthSnapshots.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customer.Id)
            .OrderByDescending(x => x.CalculatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var stageTask = dbContext.CustomerLifecycleStageHistories.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.CustomerId == customer.Id)
            .OrderByDescending(x => x.ChangedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        var enrichmentTask = dbContext.CustomerEnrichmentProfiles.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var auditTask = dbContext.CustomerAuditEvents.AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.EntityType == CustomerEntityType.Customer && x.EntityId == customer.Id)
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        var openLeads = await leadProvider.GetOpenLeadsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var openOpportunities = await opportunityProvider.GetOpenOpportunitiesAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var openDeals = await dealProvider.GetOpenDealsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var quotes = await quoteProvider.GetQuotesAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var tickets = await ticketProvider.GetTicketsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var finance = await financeProvider.GetFinanceSummaryAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var contracts = await contractProvider.GetContractsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var documents = await documentProvider.GetDocumentsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var activities = await activityProvider.GetActivitiesAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var communications = await communicationProvider.GetCommunicationsAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var hierarchy = await hierarchyProvider.GetHierarchyAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);
        var stakeholderMap = await stakeholderMapProvider.GetStakeholdersAsync(tenantId, customer.Id, customer.CompanyId, cancellationToken);

        var consents = await consentTask;
        var quality = await qualityTask;
        var health = await healthTask;
        var stage = await stageTask;
        var enrichment = await enrichmentTask;
        var audits = await auditTask;
        var duplicates = await duplicateDetectionService.FindCustomerDuplicatesAsync(customer, cancellationToken);

        var timeline = BuildTimeline(customer, audits, openLeads, openOpportunities, openDeals, quotes, tickets, documents, activities, communications);
        var nextActions = BuildNextActions(consents, quality, health, duplicates);

        return new Customer360Dto(
            new Customer360BasicInfoDto(
                customer.Id,
                customer.FullName,
                customer.Title,
                securityService.Mask(nameof(Customer), nameof(Customer.Email), customer.Email),
                securityService.Mask(nameof(Customer), nameof(Customer.MobilePhone), customer.MobilePhone),
                securityService.Mask(nameof(Customer), nameof(Customer.WorkPhone), customer.WorkPhone),
                securityService.Mask(nameof(Customer), nameof(Customer.IdentityNumber), customer.IdentityNumber),
                customer.IsVip,
                customer.IsActive,
                customer.CreatedAt,
                customer.UpdatedAt),
            customer.Company is null
                ? null
                : new Customer360CompanyInfoDto(
                    customer.Company.Id,
                    customer.Company.Name,
                    customer.Company.Website,
                    securityService.Mask(nameof(Company), nameof(Company.Email), customer.Company.Email),
                    securityService.Mask(nameof(Company), nameof(Company.Phone), customer.Company.Phone),
                    securityService.Mask(nameof(Company), nameof(Company.TaxNumber), customer.Company.TaxNumber),
                    customer.Company.Sector,
                    customer.Company.ParentCompanyId),
            customer.Contacts
                .OrderByDescending(x => x.IsPrimaryContact)
                .ThenBy(x => x.FirstName)
                .Select(x => new Customer360ContactDto(
                    x.Id,
                    x.FullName,
                    securityService.Mask(nameof(Contact), nameof(Contact.Email), x.Email),
                    securityService.Mask(nameof(Contact), nameof(Contact.MobilePhone), x.MobilePhone),
                    x.IsPrimaryContact,
                    x.IsActive))
                .ToList(),
            customer.Addresses
                .OrderByDescending(x => x.IsDefault)
                .Select(x => new Customer360AddressDto(x.Id, x.AddressType.ToString(), x.Line1, x.City, x.Country, x.IsDefault))
                .ToList(),
            stage?.NewStage,
            customer.OwnerUserId,
            null,
            BuildConsentSummary(consents),
            quality is null ? null : ToDto(quality),
            health is null ? null : ToDto(health),
            openLeads,
            openOpportunities,
            openDeals,
            quotes,
            tickets,
            finance,
            contracts,
            documents,
            activities,
            communications,
            timeline,
            CustomerPortalLinkBuilder.Build(portalOptions.Value, customer.Id),
            new CustomerAuditSummaryDto(audits.Count, audits.FirstOrDefault()?.OccurredAtUtc, audits.FirstOrDefault()?.ActorUserId),
            duplicates,
            nextActions,
            hierarchy,
            stakeholderMap,
            enrichment is null ? null : new CustomerEnrichmentProfileDto(enrichment.Website, enrichment.Domain, enrichment.LinkedInUrl, enrichment.Industry, enrichment.EmployeeCount, enrichment.AnnualRevenue, enrichment.Country, enrichment.City, enrichment.Source, enrichment.ConfidenceScore, enrichment.LastEnrichedAtUtc));
    }

    private static CustomerConsentSummaryDto BuildConsentSummary(IReadOnlyCollection<CustomerConsent> consents)
    {
        var now = DateTime.UtcNow;
        var channels = consents
            .Where(x => !x.ValidUntilUtc.HasValue || x.ValidUntilUtc.Value >= now)
            .Select(x => new CustomerConsentChannelStatusDto(x.Channel, x.Purpose, x.Status, x.ValidUntilUtc))
            .ToList();
        var marketingAllowed = channels.Any(x => x.Purpose == CustomerConsentPurpose.Marketing && x.Status == CustomerConsentStatus.OptedIn);
        return new CustomerConsentSummaryDto(marketingAllowed, channels);
    }

    private static CustomerDataQualityDto ToDto(CustomerDataQualitySnapshot snapshot)
        => new(
            snapshot.Score,
            snapshot.DuplicateRiskScore,
            snapshot.StaleDataRiskScore,
            ReadJsonList(snapshot.MissingFieldsJson),
            ReadJsonList(snapshot.InvalidFieldsJson),
            ReadJsonList(snapshot.RecommendationsJson),
            snapshot.CalculatedAtUtc);

    private static CustomerRelationshipHealthDto ToDto(CustomerRelationshipHealthSnapshot snapshot)
        => new(snapshot.Score, snapshot.RiskLevel, snapshot.LastActivityAtUtc, snapshot.OpenTicketCount, snapshot.OverdueTicketCount, snapshot.OpenOpportunityCount, snapshot.UnpaidInvoiceCount, snapshot.CalculatedAtUtc);

    private static IReadOnlyList<string> ReadJsonList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<string>>(json) ?? []; }
        catch (JsonException) { return []; }
    }

    private static IReadOnlyList<Customer360TimelineItemDto> BuildTimeline(Customer customer, IReadOnlyList<CustomerAuditEvent> audits, params IReadOnlyList<Customer360SummaryItemDto>[] summaries)
    {
        var items = new List<Customer360TimelineItemDto>
        {
            new(customer.CreatedAt, "CustomerManagement", "Created", "Created customer", customer.Id, null)
        };
        items.AddRange(audits.Select(x => new Customer360TimelineItemDto(x.OccurredAtUtc, "Audit", x.Action.ToString(), x.FieldName ?? x.Action.ToString(), x.EntityId, x.MetadataJson)));
        foreach (var summary in summaries)
            items.AddRange(summary.Where(x => x.OccurredAtUtc.HasValue).Select(x => new Customer360TimelineItemDto(x.OccurredAtUtc!.Value, "Related", x.Status ?? "Updated", x.Title, x.Id, null)));
        return items.OrderByDescending(x => x.OccurredAtUtc).Take(100).ToList();
    }

    private static IReadOnlyList<Customer360NextActionDto> BuildNextActions(IReadOnlyCollection<CustomerConsent> consents, CustomerDataQualitySnapshot? quality, CustomerRelationshipHealthSnapshot? health, IReadOnlyCollection<CustomerDuplicateWarningDto> duplicates)
    {
        var actions = new List<Customer360NextActionDto>();
        if (!consents.Any(x => x.Purpose == CustomerConsentPurpose.Marketing && x.Status == CustomerConsentStatus.OptedIn))
            actions.Add(new Customer360NextActionDto("consent.capture", "Capture marketing consent", "Medium"));
        if (quality is null || quality.Score < 70)
            actions.Add(new Customer360NextActionDto("data-quality.review", "Review missing or invalid customer data", "High"));
        if (health is not null && health.RiskLevel >= CustomerRelationshipRiskLevel.High)
            actions.Add(new Customer360NextActionDto("health.follow-up", "Schedule relationship follow-up", "High"));
        if (duplicates.Count > 0)
            actions.Add(new Customer360NextActionDto("duplicates.review", "Review duplicate customer warnings", "High"));
        return actions;
    }
}
