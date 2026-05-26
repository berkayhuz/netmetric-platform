// <copyright file="GetSalesQuotasQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetSalesQuotasQueryHandler(ISalesForecastingDbContext dbContext) : IRequestHandler<GetSalesQuotasQuery, IReadOnlyList<SalesQuotaDto>>
{
    public async Task<IReadOnlyList<SalesQuotaDto>> Handle(GetSalesQuotasQuery request, CancellationToken cancellationToken)
    {
        var quotas = await SalesForecastingQueryHelper.BuildQuotaQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId)
            .OrderBy(x => x.OwnerUserId)
            .ToListAsync(cancellationToken);

        return quotas.Select(SalesForecastingMappings.ToDto).ToList();
    }
}
