// <copyright file="LostReasonsController.cs" company="NetMetric">
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
[Route("api/pipeline/lost-reasons")]
[Authorize(Policy = PipelineManagementAuthorizationPolicies.LostReasonsRead)]
public sealed class LostReasonsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LostReasonDto>>> Get(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLostReasonsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.LostReasonsManage)]
    public async Task<ActionResult<LostReasonDto>> Create([FromBody] LostReasonUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateLostReasonCommand(request.Name, request.Description, request.IsDefault), cancellationToken);
        return CreatedAtAction(nameof(Get), new { lostReasonId = result.Id }, result);
    }

    [HttpPut("{lostReasonId:guid}")]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.LostReasonsManage)]
    public async Task<ActionResult<LostReasonDto>> Update(
        Guid lostReasonId,
        [FromBody] LostReasonUpsertRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RowVersion))
        {
            return BadRequest("A row version is required for lost reason updates.");
        }

        var result = await mediator.Send(
            new UpdateLostReasonCommand(lostReasonId, request.Name, request.Description, request.IsDefault, request.RowVersion),
            cancellationToken);

        return Ok(result);
    }
}
