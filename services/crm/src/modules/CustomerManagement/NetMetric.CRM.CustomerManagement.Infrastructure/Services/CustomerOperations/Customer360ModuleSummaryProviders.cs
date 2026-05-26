// <copyright file="Customer360ModuleSummaryProviders.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;

public sealed class LeadManagementCustomerSummaryProvider(ILeadManagementDbContext dbContext) : ICustomerLeadSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenLeadsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Leads.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.ConvertedCustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .Where(x => x.Status != LeadStatusType.Converted && x.Status != LeadStatusType.Won && x.Status != LeadStatusType.Lost && x.Status != LeadStatusType.Archived)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(
                x.Id,
                string.IsNullOrWhiteSpace(x.FullName) ? string.Concat(x.FirstName, " ", x.LastName).Trim() : x.FullName,
                x.Status.ToString(),
                x.CreatedAt,
                x.EstimatedBudget,
                null))
            .ToListAsync(cancellationToken);
}

public sealed class OpportunityManagementCustomerSummaryProvider(IOpportunityManagementDbContext dbContext) : ICustomerOpportunitySummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenOpportunitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Opportunities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .Where(x => x.Status == OpportunityStatusType.Open)
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Name, x.Stage.ToString(), x.ExpectedCloseDate ?? x.CreatedAt, x.EstimatedAmount, null))
            .ToListAsync(cancellationToken);
}

public sealed class DealManagementCustomerSummaryProvider(IDealManagementDbContext dbContext) : ICustomerDealSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetOpenDealsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Deals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .Where(x => x.ClosedAt == null && (x.Outcome == null || (x.Outcome != "Won" && x.Outcome != "Lost")))
            .OrderByDescending(x => x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Name, x.Stage ?? x.Outcome, x.ClosedDate == default ? x.CreatedAt : x.ClosedDate, x.TotalAmount == 0 ? x.Amount : x.TotalAmount, null))
            .ToListAsync(cancellationToken);
}

public sealed class QuoteManagementCustomerSummaryProvider(IQuoteManagementDbContext dbContext) : ICustomerQuoteSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetQuotesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Quotes.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive && x.CustomerId == customerId)
            .OrderByDescending(x => x.QuoteDate == default ? x.CreatedAt : x.QuoteDate)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, string.IsNullOrWhiteSpace(x.Title) ? x.QuoteNumber : x.Title!, x.Status.ToString(), x.QuoteDate == default ? x.CreatedAt : x.QuoteDate, x.GrandTotal, null))
            .ToListAsync(cancellationToken);
}

public sealed class TicketManagementCustomerSummaryProvider(ITicketManagementDbContext dbContext) : ICustomerTicketSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetTicketsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
        => await dbContext.Tickets.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive && x.CustomerId == customerId)
            .Where(x => x.Status == TicketStatusType.New || x.Status == TicketStatusType.Open || x.Status == TicketStatusType.Pending)
            .OrderByDescending(x => x.OpenedAt == default ? x.CreatedAt : x.OpenedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Subject, x.Status.ToString(), x.OpenedAt == default ? x.CreatedAt : x.OpenedAt, null, null))
            .ToListAsync(cancellationToken);
}

public sealed class WorkManagementCustomerActivitySummaryProvider(
    IWorkManagementDbContext workDbContext,
    IOpportunityManagementDbContext opportunityDbContext) : ICustomerActivitySummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetActivitiesAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        var workActivities = await workDbContext.Activities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.RelatedEntityId == customerId || (companyId.HasValue && x.RelatedEntityId == companyId.Value))
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(20)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Subject, x.ActivityType.ToString(), x.OccurredAtUtc, null, null))
            .ToListAsync(cancellationToken);

        var sharedActivities = await opportunityDbContext.Activities.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .OrderByDescending(x => x.StartDate ?? x.CreatedAt)
            .Take(20)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Subject, "Activity", x.StartDate ?? x.CreatedAt, null, null))
            .ToListAsync(cancellationToken);

        return workActivities.Concat(sharedActivities)
            .OrderByDescending(x => x.OccurredAtUtc ?? DateTime.MinValue)
            .Take(20)
            .ToList();
    }
}

public sealed class SupportInboxCustomerCommunicationSummaryProvider(
    ICustomerManagementDbContext customerManagementDbContext,
    ISupportInboxIntegrationDbContext inboxDbContext) : ICustomerCommunicationSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetCommunicationsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        var email = await customerManagementDbContext.Customers.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.Id == customerId)
            .Select(x => x.Email)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(email))
            return [];

        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await inboxDbContext.Messages.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.FromAddress.Equals(normalizedEmail, StringComparison.CurrentCultureIgnoreCase))
            .OrderByDescending(x => x.ReceivedAtUtc)
            .Take(20)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Subject, x.ProcessingStatus.ToString(), x.ReceivedAtUtc, null, null))
            .ToListAsync(cancellationToken);
    }
}

public sealed class FinanceOperationsCustomerSummaryProvider(IFinanceOperationsDbContext dbContext) : ICustomerFinanceSummaryProvider
{
    public async Task<CustomerFinanceSummaryDto> GetFinanceSummaryAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        var invoices = await dbContext.Invoices.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .ToListAsync(cancellationToken);
        var payments = await dbContext.Payments.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var baseCurrency = invoices.Select(x => x.Currency).Concat(payments.Select(x => x.Currency)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).Take(2).ToList();
        var canAggregate = baseCurrency.Count <= 1;
        var paidStatuses = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Paid", "Closed", "Settled" };
        var openInvoices = invoices.Where(x => !paidStatuses.Contains(x.Status) && !x.PaidAtUtc.HasValue).ToList();
        var invoiceItems = invoices
            .OrderByDescending(x => x.IssuedAtUtc ?? x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Name, x.Status, x.IssuedAtUtc ?? x.CreatedAt, x.Amount, null))
            .ToList();
        var paymentItems = payments
            .OrderByDescending(x => x.PaidAtUtc ?? x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Name, x.Status, x.PaidAtUtc ?? x.CreatedAt, x.Amount, null))
            .ToList();

        return new CustomerFinanceSummaryDto(
            canAggregate ? invoices.Sum(x => x.Amount) : null,
            canAggregate ? openInvoices.Sum(x => x.Amount) : null,
            openInvoices.Count,
            openInvoices.Count(x => x.DueDateUtc.HasValue && x.DueDateUtc.Value < now),
            invoiceItems,
            paymentItems);
    }
}

public sealed class ContractLifecycleCustomerSummaryProvider(IContractLifecycleDbContext dbContext) : ICustomerContractSummaryProvider
{
    public async Task<IReadOnlyList<Customer360SummaryItemDto>> GetContractsAsync(Guid tenantId, Guid customerId, Guid? companyId, CancellationToken cancellationToken)
    {
        var contracts = await dbContext.Contracts.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .OrderByDescending(x => x.StartDateUtc ?? x.CreatedAt)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, string.IsNullOrWhiteSpace(x.ContractNumber) ? x.Name : x.ContractNumber, x.Status, x.RenewalDateUtc ?? x.EndDateUtc ?? x.CreatedAt, x.ContractValue, null))
            .ToListAsync(cancellationToken);
        var renewals = await dbContext.Renewals.AsNoTracking()
            .Where(x => x.TenantId == tenantId && !x.IsDeleted && x.IsActive)
            .Where(x => x.CustomerId == customerId || (companyId.HasValue && x.CompanyId == companyId.Value))
            .OrderBy(x => x.RenewalDateUtc ?? DateTime.MaxValue)
            .Take(10)
            .Select(x => new Customer360SummaryItemDto(x.Id, x.Name, $"Renewal:{x.Status}", x.RenewalDateUtc ?? x.CreatedAt, null, null))
            .ToListAsync(cancellationToken);
        return contracts.Concat(renewals).OrderByDescending(x => x.OccurredAtUtc ?? DateTime.MinValue).Take(10).ToList();
    }
}
