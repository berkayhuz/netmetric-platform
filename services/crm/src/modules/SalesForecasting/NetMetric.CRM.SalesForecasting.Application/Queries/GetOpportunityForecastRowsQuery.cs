// <copyright file="GetOpportunityForecastRowsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Queries;

public sealed record GetOpportunityForecastRowsQuery(DateOnly PeriodStart, DateOnly PeriodEnd, Guid? OwnerUserId) : IRequest<IReadOnlyList<OpportunityForecastRowDto>>;
