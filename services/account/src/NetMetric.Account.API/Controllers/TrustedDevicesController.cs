// <copyright file="TrustedDevicesController.cs" company="NetMetric">
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
using NetMetric.Account.Application.Devices.Commands;
using NetMetric.Account.Application.Devices.Queries;
using NetMetric.Account.Contracts.Devices;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/devices/trusted")]
public sealed class TrustedDevicesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.DevicesReadOwn)]
    [ProducesResponseType<TrustedDevicesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<TrustedDevicesResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetTrustedDevicesQuery(), cancellationToken)).ToActionResult();

    [HttpDelete("{deviceId:guid}")]
    [Authorize(Policy = AccountPolicies.DevicesRevokeOwn)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Revoke(Guid deviceId, CancellationToken cancellationToken)
        => (await mediator.Send(new RevokeTrustedDeviceCommand(deviceId), cancellationToken)).ToActionResult();

    [HttpPost("revoke-others")]
    [Authorize(Policy = AccountPolicies.DevicesRevokeOwn)]
    [EnableRateLimiting(AccountOperationalHardeningExtensions.CriticalRateLimitPolicy)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RevokeOthers(CancellationToken cancellationToken)
        => (await mediator.Send(new RevokeOtherTrustedDevicesCommand(), cancellationToken)).ToActionResult();
}
