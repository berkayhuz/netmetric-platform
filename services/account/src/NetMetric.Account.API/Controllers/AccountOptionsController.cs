// <copyright file="AccountOptionsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Preferences.Queries;
using NetMetric.Account.Contracts.Preferences;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/options")]
public sealed class AccountOptionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.PreferencesReadOwn)]
    [ProducesResponseType<AccountOptionsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountOptionsResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetAccountOptionsQuery(), cancellationToken)).ToActionResult();
}
