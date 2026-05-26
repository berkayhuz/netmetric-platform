// <copyright file="ICustomerPortalSummaryMetricsProvider.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Insights;

public interface ICustomerPortalSummaryMetricsProvider
{
    Task<CustomerPortalSummaryMetrics> GetMetricsAsync(Guid customerId, CancellationToken cancellationToken);
}

public sealed record CustomerPortalSummaryMetrics(
    int OpenTickets,
    int OpenOpportunities,
    int OpenInvoices);
