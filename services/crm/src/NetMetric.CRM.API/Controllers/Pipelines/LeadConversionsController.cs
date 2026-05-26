// <copyright file="LeadConversionsController.cs" company="NetMetric">
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
[Route("api/pipeline/lead-conversions")]
[Authorize(Policy = PipelineManagementAuthorizationPolicies.LeadConversionsRead)]
public sealed class LeadConversionsController(IMediator mediator) : ControllerBase
{
    [HttpGet("{leadId:guid}/preview")]
    public async Task<ActionResult<LeadConversionPreviewDto>> Preview(Guid leadId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetLeadConversionPreviewQuery(leadId), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{leadId:guid}")]
    [Authorize(Policy = PipelineManagementAuthorizationPolicies.LeadConversionsManage)]
    public async Task<ActionResult<LeadConversionResultDto>> Convert(
        Guid leadId,
        [FromBody] ConvertLeadRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new ConvertLeadCommand(
                leadId,
                request.CreateCustomer,
                request.CreateOpportunity,
                request.ExistingCustomerId,
                request.OpportunityName,
                request.EstimatedAmount,
                request.InitialStage,
                request.Priority,
                request.OwnerUserId,
                request.Notes),
            cancellationToken);

        return Ok(result);
    }
}
