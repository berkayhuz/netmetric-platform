// <copyright file="NotificationsController.cs" company="NetMetric">
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
[Route("api/v1/account/notifications")]
public sealed class NotificationsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AccountPolicies.NotificationsReadOwn)]
    [ProducesResponseType<AccountNotificationsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AccountNotificationsResponse>> Get([FromQuery] string? filter, CancellationToken cancellationToken)
        => (await mediator.Send(new GetMyNotificationsQuery(filter), cancellationToken)).ToActionResult();

    [HttpPost("{notificationId:guid}/read")]
    [Authorize(Policy = AccountPolicies.NotificationsUpdateOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAsRead(Guid notificationId, CancellationToken cancellationToken)
        => (await mediator.Send(new MarkNotificationAsReadCommand(notificationId), cancellationToken)).ToActionResult();

    [HttpPost("mark-all-read")]
    [Authorize(Policy = AccountPolicies.NotificationsUpdateOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> MarkAllAsRead(CancellationToken cancellationToken)
        => (await mediator.Send(new MarkAllNotificationsAsReadCommand(), cancellationToken)).ToActionResult();

    [HttpDelete("{notificationId:guid}")]
    [Authorize(Policy = AccountPolicies.NotificationsUpdateOwn)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid notificationId, CancellationToken cancellationToken)
        => (await mediator.Send(new DeleteNotificationCommand(notificationId), cancellationToken)).ToActionResult();
}
