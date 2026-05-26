// <copyright file="PipelineOpportunitiesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Application.Security;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.PipelineManagement.Contracts.Requests;

namespace NetMetric.CRM.API.Controllers.Pipelines;

[ApiController]
[Route("api/opportunities/pipelines/items")]
[Authorize(Policy = PipelineManagementAuthorizationPolicies.OpportunityStageHistoryRead)]
public sealed class PipelineOpportunitiesController(IMediator mediator) : ControllerBase
{
    [HttpGet("{opportunityId:guid}/stage-history")]
    public async Task<ActionResult<IReadOnlyList<OpportunityStageHistoryDto>>> GetStageHistory(
        Guid opportunityId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOpportunityStageHistoryQuery(opportunityId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{opportunityId:guid}/stage")]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.OpportunityPipelineManage)]
    public async Task<ActionResult<OpportunityStageTransitionResultDto>> ChangeStage(
        Guid opportunityId,
        [FromBody] ChangeOpportunityStageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ChangeOpportunityStageCommand(
                opportunityId,
                request.NewStage,
                request.NewPipelineStageId,
                request.Note,
                request.LostReasonId,
                request.LostNote,
                request.RowVersion),
            cancellationToken);

        return Ok(result);
    }
}
