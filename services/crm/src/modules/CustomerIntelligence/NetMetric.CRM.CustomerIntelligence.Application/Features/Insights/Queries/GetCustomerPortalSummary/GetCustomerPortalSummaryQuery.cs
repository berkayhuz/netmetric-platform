// <copyright file="GetCustomerPortalSummaryQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Insights.Queries.GetCustomerPortalSummary;

public sealed record GetCustomerPortalSummaryQuery(Guid CustomerId) : IRequest<CustomerPortalSummaryDto>;
