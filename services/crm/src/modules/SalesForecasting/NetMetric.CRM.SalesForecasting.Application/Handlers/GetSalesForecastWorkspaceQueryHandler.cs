// <copyright file="GetSalesForecastWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetSalesForecastWorkspaceQueryHandler(IMediator mediator) : IRequestHandler<GetSalesForecastWorkspaceQuery, SalesForecastWorkspaceDto>
{
    public async Task<SalesForecastWorkspaceDto> Handle(GetSalesForecastWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var summary = await mediator.Send(new GetSalesForecastSummaryQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);
        var opportunities = await mediator.Send(new GetOpportunityForecastRowsQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);
        var quotas = await mediator.Send(new GetSalesQuotasQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);
        var adjustments = await mediator.Send(new GetForecastAdjustmentsQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);
        var snapshots = await mediator.Send(new GetForecastSnapshotsQuery(request.PeriodStart, request.PeriodEnd, request.OwnerUserId), cancellationToken);

        return new SalesForecastWorkspaceDto(summary, opportunities, quotas, adjustments, snapshots);
    }
}
