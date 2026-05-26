// <copyright file="AccountOverviewController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Overview.Queries;
using NetMetric.Account.Contracts.Overview;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/overview")]
[Authorize(Policy = AccountPolicies.AccountRead)]
public sealed class AccountOverviewController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AccountOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountOverviewResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetAccountOverviewQuery(), cancellationToken)).ToActionResult();
}
