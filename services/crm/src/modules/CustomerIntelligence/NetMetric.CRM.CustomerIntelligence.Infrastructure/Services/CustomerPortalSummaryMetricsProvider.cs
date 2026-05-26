// <copyright file="CustomerPortalSummaryMetricsProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Insights;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.Services;

public sealed class CustomerPortalSummaryMetricsProvider(
    ITicketManagementDbContext ticketDbContext,
    IOpportunityManagementDbContext opportunityDbContext,
    IFinanceOperationsDbContext financeDbContext) : ICustomerPortalSummaryMetricsProvider
{
    public async Task<CustomerPortalSummaryMetrics> GetMetricsAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var openTicketsTask = ticketDbContext.Tickets
            .AsNoTracking()
            .CountAsync(
                x => x.CustomerId == customerId &&
                     x.Status != TicketStatusType.Closed &&
                     x.Status != TicketStatusType.Resolved &&
                     x.Status != TicketStatusType.Cancelled,
                cancellationToken);

        var openOpportunitiesTask = opportunityDbContext.Opportunities
            .AsNoTracking()
            .CountAsync(
                x => x.CustomerId == customerId &&
                     x.Stage != OpportunityStageType.Won &&
                     x.Stage != OpportunityStageType.Lost,
                cancellationToken);

        var openInvoicesTask = financeDbContext.Invoices
            .AsNoTracking()
            .CountAsync(
                x => x.CustomerId == customerId &&
                     x.Status != "Paid" &&
                     x.Status != "Cancelled" &&
                     x.Status != "Canceled" &&
                     x.Status != "Void",
                cancellationToken);

        await Task.WhenAll(openTicketsTask, openOpportunitiesTask, openInvoicesTask);

        return new CustomerPortalSummaryMetrics(
            openTicketsTask.Result,
            openOpportunitiesTask.Result,
            openInvoicesTask.Result);
    }
}
