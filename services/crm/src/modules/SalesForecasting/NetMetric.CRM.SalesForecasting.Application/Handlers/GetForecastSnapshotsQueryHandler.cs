// <copyright file="GetForecastSnapshotsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetForecastSnapshotsQueryHandler(ISalesForecastingDbContext dbContext) : IRequestHandler<GetForecastSnapshotsQuery, IReadOnlyList<ForecastSnapshotDto>>
{
    public async Task<IReadOnlyList<ForecastSnapshotDto>> Handle(GetForecastSnapshotsQuery request, CancellationToken cancellationToken)
    {
        var snapshots = await SalesForecastingQueryHelper.BuildSnapshotQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId)
            .ToListAsync(cancellationToken);

        return snapshots.Select(SalesForecastingMappings.ToDto).ToList();
    }
}
