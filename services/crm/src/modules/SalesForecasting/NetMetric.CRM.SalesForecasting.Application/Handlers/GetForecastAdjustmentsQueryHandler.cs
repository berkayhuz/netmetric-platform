// <copyright file="GetForecastAdjustmentsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetForecastAdjustmentsQueryHandler(ISalesForecastingDbContext dbContext) : IRequestHandler<GetForecastAdjustmentsQuery, IReadOnlyList<ForecastAdjustmentDto>>
{
    public async Task<IReadOnlyList<ForecastAdjustmentDto>> Handle(GetForecastAdjustmentsQuery request, CancellationToken cancellationToken)
    {
        var adjustments = await SalesForecastingQueryHelper.BuildAdjustmentQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return adjustments.Select(SalesForecastingMappings.ToDto).ToList();
    }
}
