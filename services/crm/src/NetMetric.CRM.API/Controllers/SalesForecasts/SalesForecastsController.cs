// <copyright file="SalesForecastsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.SalesForecasting.Application.Commands;
using NetMetric.CRM.SalesForecasting.Application.Queries;

namespace NetMetric.CRM.API.Controllers.SalesForecasts;

[ApiController]
[Route("api/sales-forecasts")]
[Authorize(Policy = AuthorizationPolicies.SalesForecastsRead)]
public sealed class SalesForecastsController(IMediator mediator) : ControllerBase
{
    [HttpGet("workspace")]
    public async Task<IActionResult> GetWorkspace(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSalesForecastWorkspaceQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSalesForecastSummaryQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpGet("opportunity-rows")]
    public async Task<IActionResult> GetOpportunityRows(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetOpportunityForecastRowsQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpGet("quotas")]
    public async Task<IActionResult> GetQuotas(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSalesQuotasQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpPut("quotas")]
    [Authorize(Policy = AuthorizationPolicies.SalesForecastsManage)]
    public async Task<IActionResult> UpsertQuota([FromBody] UpsertSalesQuotaRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpsertSalesQuotaCommand(request.PeriodStart, request.PeriodEnd, request.OwnerUserId, request.Amount, request.CurrencyCode, request.Notes, request.RowVersion), cancellationToken));

    [HttpGet("adjustments")]
    public async Task<IActionResult> GetAdjustments(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetForecastAdjustmentsQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpPost("adjustments")]
    [Authorize(Policy = AuthorizationPolicies.SalesForecastsManage)]
    public async Task<IActionResult> CreateAdjustment([FromBody] CreateForecastAdjustmentRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateForecastAdjustmentCommand(request.PeriodStart, request.PeriodEnd, request.OwnerUserId, request.Amount, request.Reason, request.Notes), cancellationToken));

    [HttpGet("snapshots")]
    public async Task<IActionResult> GetSnapshots(DateOnly periodStart, DateOnly periodEnd, Guid? ownerUserId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetForecastSnapshotsQuery(periodStart, periodEnd, ownerUserId), cancellationToken));

    [HttpPost("snapshots")]
    [Authorize(Policy = AuthorizationPolicies.SalesForecastsManage)]
    public async Task<IActionResult> CreateSnapshot([FromBody] CreateForecastSnapshotRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateForecastSnapshotCommand(request.Name, request.PeriodStart, request.PeriodEnd, request.OwnerUserId, request.ForecastCategory, request.Notes), cancellationToken));

    public sealed record UpsertSalesQuotaRequest([property: JsonRequired] DateOnly PeriodStart, [property: JsonRequired] DateOnly PeriodEnd, Guid? OwnerUserId, [property: JsonRequired] decimal Amount, string CurrencyCode, string? Notes, string? RowVersion);

    public sealed record CreateForecastAdjustmentRequest([property: JsonRequired] DateOnly PeriodStart, [property: JsonRequired] DateOnly PeriodEnd, Guid? OwnerUserId, [property: JsonRequired] decimal Amount, string Reason, string? Notes);

    public sealed record CreateForecastSnapshotRequest(string Name, [property: JsonRequired] DateOnly PeriodStart, [property: JsonRequired] DateOnly PeriodEnd, Guid? OwnerUserId, string ForecastCategory, string? Notes);
}
