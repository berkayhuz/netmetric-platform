// <copyright file="CustomerPortalSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed class CustomerPortalSummaryDto
{
    public required Guid CustomerId { get; init; }
    public required string DisplayName { get; init; }
    public required decimal HealthScore { get; init; }
    public required int OpenTickets { get; init; }
    public required int OpenOpportunities { get; init; }
    public required int OpenInvoices { get; init; }
}
