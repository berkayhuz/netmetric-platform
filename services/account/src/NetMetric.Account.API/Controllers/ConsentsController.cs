// <copyright file="ConsentsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Consents.Commands;
using NetMetric.Account.Application.Consents.Queries;
using NetMetric.Account.Contracts.Consents;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/consents")]
public sealed class ConsentsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.ConsentsReadOwn)]
    [ProducesResponseType<ConsentsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ConsentsResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMyConsentsQuery(), cancellationToken)).ToActionResult();

    [HttpPost("{consentType}/accept")]
    [Authorize(Policy = AccountPolicies.ConsentsAcceptOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Accept(
        string consentType,
        [FromBody] AcceptConsentRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new AcceptConsentCommand(consentType, request.Version), cancellationToken)).ToActionResult();
}
