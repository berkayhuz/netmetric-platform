// <copyright file="SessionsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using NetMetric.Account.Api.DependencyInjection;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Sessions.Commands;
using NetMetric.Account.Application.Sessions.Queries;
using NetMetric.Account.Contracts.Sessions;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/sessions")]
public sealed class SessionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.SessionsReadOwn)]
    [ProducesResponseType<UserSessionsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<UserSessionsResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMySessionsQuery(), cancellationToken)).ToActionResult();

    [HttpDelete("{sessionId:guid}")]
    [Authorize(Policy = AccountPolicies.SessionsRevokeOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Revoke(Guid sessionId, CancellationToken cancellationToken)
        => (await mediator.Send(new RevokeSessionCommand(sessionId), cancellationToken)).ToActionResult();

    [HttpPost("revoke-others")]
    [Authorize(Policy = AccountPolicies.SessionsRevokeOwn)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RevokeOthers(CancellationToken cancellationToken)
        => (await mediator.Send(new RevokeOtherSessionsCommand(), cancellationToken)).ToActionResult();
}
