// <copyright file="AnalyticsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetAnalyticsProjectionStatus;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetCampaignRoi;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRevenueAging;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRoleDashboard;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetSalesFunnelSummary;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetSupportKpis;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetTenantReportSummary;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetUserProductivity;
using NetMetric.CRM.API.Compatibility;

namespace NetMetric.CRM.API.Controllers.Analytics;

[ApiController]
[Route("api/analytics")]
[Authorize(Policy = AuthorizationPolicies.AnalyticsRead)]
public sealed class AnalyticsController(IMediator mediator) : ControllerBase
{
    [HttpGet("tenants/{tenantId:guid}/summary")]
    public async Task<IActionResult> GetTenantSummary(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTenantReportSummaryQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/sales-funnel")]
    public async Task<IActionResult> GetSalesFunnel(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSalesFunnelSummaryQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/campaign-roi")]
    public async Task<IActionResult> GetCampaignRoi(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCampaignRoiQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/revenue-aging")]
    public async Task<IActionResult> GetRevenueAging(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRevenueAgingQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/support-kpis")]
    public async Task<IActionResult> GetSupportKpis(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSupportKpisQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/user-productivity")]
    public async Task<IActionResult> GetUserProductivity(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetUserProductivityQuery(tenantId), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/projection-status")]
    public async Task<IActionResult> GetProjectionStatus(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetAnalyticsProjectionStatusQuery(tenantId), cancellationToken));

    [HttpGet("dashboards/roles/{roleName}")]
    public async Task<IActionResult> GetRoleDashboard(string roleName, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetRoleDashboardQuery(roleName), cancellationToken));
}
