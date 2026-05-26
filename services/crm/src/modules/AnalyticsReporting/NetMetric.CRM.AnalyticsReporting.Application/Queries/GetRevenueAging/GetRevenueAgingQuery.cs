// <copyright file="GetRevenueAgingQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.AnalyticsReporting.Application.DTOs;

namespace NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRevenueAging;

public sealed record GetRevenueAgingQuery(Guid TenantId) : IRequest<IReadOnlyCollection<RevenueAgingDto>>;
