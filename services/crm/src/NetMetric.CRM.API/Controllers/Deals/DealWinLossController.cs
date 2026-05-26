// <copyright file="DealWinLossController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.DealManagement.Application.Commands.Reviews;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CRM.DealManagement.Contracts.Requests;

namespace NetMetric.CRM.API.Controllers.Deals;

[ApiController]
[Route("api/deals/win-loss")]
[Authorize(Policy = AuthorizationPolicies.WinLossRead)]
public sealed class DealWinLossController(IMediator mediator) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult<WinLossSummaryDto>> GetSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] Guid? ownerUserId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetWinLossSummaryQuery(from, to, ownerUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("lost-reasons")]
    public async Task<ActionResult<IReadOnlyList<LostReasonDto>>> GetLostReasons(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLostReasonsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{dealId:guid}/review")]
    [Authorize(Policy = AuthorizationPolicies.WinLossManage)]
    public async Task<ActionResult<WinLossReviewDto>> UpsertReview(
        Guid dealId,
        [FromBody] DealReviewUpsertRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpsertWinLossReviewCommand(
                dealId,
                request.Outcome,
                request.Summary,
                request.Strengths,
                request.Risks,
                request.CompetitorName,
                request.CompetitorPrice,
                request.CustomerFeedback,
                request.RowVersion),
            cancellationToken);

        return Ok(result);
    }
}
