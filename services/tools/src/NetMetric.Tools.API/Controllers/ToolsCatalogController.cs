// <copyright file="ToolsCatalogController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.Tools.API.DependencyInjection;
using NetMetric.Tools.Application.Catalog.Queries;
using NetMetric.Tools.Contracts.Catalog;

namespace NetMetric.Tools.API.Controllers;

[ApiController]
[Route("api/v1/tools/catalog")]
public sealed class ToolsCatalogController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    [EnableRateLimiting("tools-public-catalog")]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType<ToolCatalogResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ToolCatalogResponse>> GetPublicCatalog([FromQuery] string? category, [FromQuery] string? q, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetToolsCatalogQuery(category, q, true), cancellationToken));

    [HttpGet("private")]
    [Authorize(Policy = ToolsPolicies.ToolsHistoryReadOwn)]
    [ProducesResponseType<ToolCatalogResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ToolCatalogResponse>> GetPrivateCatalog([FromQuery] string? category, [FromQuery] string? q, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetToolsCatalogQuery(category, q, false), cancellationToken));

    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ToolDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ToolDetailResponse>> GetDetail([FromRoute] string slug, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(new GetToolDetailQuery(slug), cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
}
