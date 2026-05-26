// <copyright file="NotificationPreferencesController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Notifications.Commands;
using NetMetric.Account.Application.Notifications.Queries;
using NetMetric.Account.Contracts.Notifications;

namespace NetMetric.Account.Api.Controllers;

[ApiController]
[Route("api/v1/account/notifications/preferences")]
public sealed class NotificationPreferencesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.NotificationsReadOwn)]
    [ProducesResponseType<NotificationPreferencesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesResponse>> Get(CancellationToken cancellationToken)
        => (await mediator.Send(new GetMyNotificationPreferencesQuery(), cancellationToken)).ToActionResult();

    [HttpPut]
    [Authorize(Policy = AccountPolicies.NotificationsUpdateOwn)]
    [ProducesResponseType<NotificationPreferencesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesResponse>> Update(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken)
        => (await mediator.Send(new UpdateMyNotificationPreferencesCommand(request), cancellationToken)).ToActionResult();
}
