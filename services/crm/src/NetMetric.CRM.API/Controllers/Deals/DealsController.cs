// <copyright file="DealsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CRM.DealManagement.Contracts.Requests;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.Deals;

[ApiController]
[Route("api/deals")]
[Authorize(Policy = AuthorizationPolicies.DealsRead)]
public sealed class DealsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<DealListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] Guid? ownerUserId,
        [FromQuery] Guid? companyId,
        [FromQuery] Guid? opportunityId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetDealsQuery(search, ownerUserId, companyId, opportunityId, isActive, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{dealId:guid}")]
    public async Task<ActionResult<DealDetailDto>> GetById(Guid dealId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDealByIdQuery(dealId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{dealId:guid}/workspace")]
    public async Task<ActionResult<DealWorkspaceDto>> GetWorkspace(Guid dealId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDealWorkspaceQuery(dealId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{dealId:guid}/timeline")]
    public async Task<ActionResult<IReadOnlyList<DealOutcomeHistoryDto>>> GetTimeline(Guid dealId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDealTimelineQuery(dealId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<ActionResult<DealDetailDto>> Create([FromBody] DealUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateDealCommand(
                request.DealCode,
                request.Name,
                request.TotalAmount,
                request.ClosedDate,
                request.OpportunityId,
                request.CompanyId,
                request.OwnerUserId,
                request.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { dealId = result.Id }, result);
    }

    [HttpPut("{dealId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<ActionResult<DealDetailDto>> Update(Guid dealId, [FromBody] DealUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateDealCommand(
                dealId,
                request.DealCode,
                request.Name,
                request.TotalAmount,
                request.ClosedDate,
                request.OpportunityId,
                request.CompanyId,
                request.OwnerUserId,
                request.Notes,
                request.RowVersion ?? string.Empty),
            cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{dealId:guid}/owner")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<IActionResult> AssignOwner(Guid dealId, [FromBody] AssignDealOwnerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignDealOwnerCommand(dealId, request.OwnerUserId), cancellationToken);
        return NoContent();
    }

    [HttpPatch("owner")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<ActionResult<BulkOperationResult>> BulkAssignOwner([FromBody] BulkAssignDealsOwnerRequest request, CancellationToken cancellationToken)
    {
        var affected = await mediator.Send(new BulkAssignDealsOwnerCommand(request.DealIds, request.OwnerUserId), cancellationToken);
        return Ok(new BulkOperationResult(affected));
    }

    [HttpPost("{dealId:guid}/won")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<IActionResult> MarkWon(Guid dealId, [FromBody] DealOutcomeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkDealWonCommand(dealId, request.OccurredAt, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{dealId:guid}/lost")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<IActionResult> MarkLost(Guid dealId, [FromBody] DealOutcomeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new MarkDealLostCommand(dealId, request.OccurredAt, request.LostReasonId, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpPost("{dealId:guid}/reopen")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<IActionResult> Reopen(Guid dealId, [FromBody] DealOutcomeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new ReopenDealCommand(dealId, request.Note, request.RowVersion), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{dealId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.DealsManage)]
    public async Task<IActionResult> Delete(Guid dealId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteDealCommand(dealId), cancellationToken);
        return NoContent();
    }

    public sealed record BulkOperationResult([property: JsonRequired] int AffectedCount);
}
