// <copyright file="TenantsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.TenantManagement.Application.Commands.ProvisionTenant;
using NetMetric.CRM.TenantManagement.Application.Commands.ToggleFeatureFlag;
using NetMetric.CRM.TenantManagement.Application.Commands.ToggleModule;
using NetMetric.CRM.TenantManagement.Application.Commands.UpdateTenantBranding;
using NetMetric.CRM.TenantManagement.Application.Queries.GetTenantSummary;

namespace NetMetric.CRM.API.Controllers.Tenants;

[ApiController]
[Route("api/tenants")]
[Authorize(Policy = AuthorizationPolicies.TenantsRead)]
public sealed class TenantsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{tenantId:guid}/summary")]
    public async Task<IActionResult> GetSummary(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTenantSummaryQuery(tenantId), cancellationToken));

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TenantsManage)]
    public async Task<IActionResult> Provision([FromBody] ProvisionTenantRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new ProvisionTenantCommand(request.TenantId, request.Name, request.AdminEmail), cancellationToken);
        return CreatedAtAction(nameof(GetSummary), new { tenantId = id }, new { id });
    }

    [HttpPut("{tenantId:guid}/branding")]
    [Authorize(Policy = AuthorizationPolicies.TenantsManage)]
    public async Task<IActionResult> UpdateBranding(Guid tenantId, [FromBody] UpdateTenantBrandingRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTenantBrandingCommand(tenantId, request.PrimaryDomain, request.Locale, request.TimeZone, request.BrandPrimaryColor, request.LogoUrl), cancellationToken);
        return NoContent();
    }

    [HttpPut("{tenantId:guid}/feature-flags/{key}")]
    [Authorize(Policy = AuthorizationPolicies.TenantsManage)]
    public async Task<IActionResult> ToggleFeatureFlag(Guid tenantId, string key, [FromBody] ToggleFeatureFlagRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ToggleFeatureFlagCommand(tenantId, key, request.IsEnabled, request.EffectiveFromUtc), cancellationToken);
        return NoContent();
    }

    [HttpPut("{tenantId:guid}/modules/{moduleKey}")]
    [Authorize(Policy = AuthorizationPolicies.TenantsManage)]
    public async Task<IActionResult> ToggleModule(Guid tenantId, string moduleKey, [FromBody] ToggleModuleRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ToggleModuleCommand(tenantId, moduleKey, request.IsEnabled), cancellationToken);
        return NoContent();
    }

    public sealed record ProvisionTenantRequest([property: JsonRequired] Guid TenantId, string Name, string AdminEmail);

    public sealed record UpdateTenantBrandingRequest(string? PrimaryDomain, string Locale, string TimeZone, string? BrandPrimaryColor, string? LogoUrl);

    public sealed record ToggleFeatureFlagRequest([property: JsonRequired] bool IsEnabled, DateTime? EffectiveFromUtc);

    public sealed record ToggleModuleRequest([property: JsonRequired] bool IsEnabled);
}
