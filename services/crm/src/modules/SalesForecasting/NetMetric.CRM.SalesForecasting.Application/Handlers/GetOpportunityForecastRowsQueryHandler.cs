// <copyright file="GetOpportunityForecastRowsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetOpportunityForecastRowsQueryHandler(ISalesForecastingDbContext dbContext) : IRequestHandler<GetOpportunityForecastRowsQuery, IReadOnlyList<OpportunityForecastRowDto>>
{
    public async Task<IReadOnlyList<OpportunityForecastRowDto>> Handle(GetOpportunityForecastRowsQuery request, CancellationToken cancellationToken)
    {
        var opportunities = await SalesForecastingQueryHelper
            .BuildOpportunityQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId)
            .OrderByDescending(x => x.EstimatedAmount)
            .ToListAsync(cancellationToken);

        return opportunities.Select(SalesForecastingMappings.ToForecastRow).ToList();
    }
}
