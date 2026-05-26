// <copyright file="PipelinesController.cs" company="NetMetric">
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

namespace NetMetric.CRM.API.Controllers.Pipelines;

[ApiController]
[Route("api/opportunities/pipelines")]
[Authorize(Policy = PipelineManagementAuthorizationPolicies.PipelinesRead)]
public sealed class PipelinesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<PipelineSummaryDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPipelinesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pipelineId:guid}")]
    public async Task<ActionResult<PipelineDto>> Get(Guid pipelineId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPipelineByIdQuery(pipelineId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pipelineId:guid}/board")]
    public async Task<ActionResult<PipelineBoardDto>> GetBoard(Guid pipelineId, [FromQuery] Guid? ownerUserId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPipelineBoardQuery(pipelineId, ownerUserId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{pipelineId:guid}/analytics")]
    public async Task<ActionResult<PipelineAnalyticsDto>> GetAnalytics(Guid pipelineId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPipelineAnalyticsQuery(pipelineId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.PipelinesManage)]
    public async Task<ActionResult<PipelineDto>> Create([FromBody] CreatePipelineCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(Get), new { pipelineId = result.Id }, result);
    }

    [HttpPut("{pipelineId:guid}")]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.PipelinesManage)]
    public async Task<ActionResult<PipelineDto>> Update(Guid pipelineId, [FromBody] UpdatePipelineCommand command, CancellationToken cancellationToken)
    {
        if (pipelineId != command.Id) return BadRequest("Mismatched ID");
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{pipelineId:guid}")]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.PipelinesManage)]
    public async Task<IActionResult> Delete(Guid pipelineId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeletePipelineCommand(pipelineId), cancellationToken);
        return NoContent();
    }
}
