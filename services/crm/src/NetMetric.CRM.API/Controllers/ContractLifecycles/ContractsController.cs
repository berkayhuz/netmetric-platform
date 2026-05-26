// <copyright file="ContractsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Commands.CreateContractRecord;
using NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Queries.GetContractRecords;
using NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Queries.GetContractRecordById;

namespace NetMetric.CRM.API.Controllers.ContractLifecycles;

[ApiController]
[Route("api/contracts")]
[Authorize(Policy = AuthorizationPolicies.ContractsRead)]
public sealed class ContractsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetContractRecordsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{contractId:guid}")]
    public async Task<IActionResult> Get(Guid contractId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetContractRecordByIdQuery(contractId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.ContractsManage)]
    public async Task<IActionResult> Create([FromBody] CreateContractRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateContractRecordCommand(request.Code, request.Name, request.Description), cancellationToken);
        return CreatedAtAction(nameof(Get), new { contractId = result.Id }, result);
    }

    public sealed record CreateContractRequest(string Code, string Name, string? Description);
}
