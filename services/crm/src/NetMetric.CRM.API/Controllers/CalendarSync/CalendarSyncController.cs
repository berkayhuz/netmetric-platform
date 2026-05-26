// <copyright file="CalendarSyncController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.CalendarSync.Application.Commands.Connections.UpsertCalendarConnection;
using NetMetric.CRM.CalendarSync.Application.Commands.Sync.TriggerCalendarSync;
using NetMetric.CRM.CalendarSync.Application.Queries.GetCalendarOverview;
using NetMetric.CRM.CalendarSync.Domain.Enums;

namespace NetMetric.CRM.API.Controllers.CalendarSync;

[ApiController]
[Route("api/calendar-sync")]
[Authorize(Policy = AuthorizationPolicies.CalendarSyncRead)]
public sealed class CalendarSyncController(IMediator mediator) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetCalendarOverviewQuery(), cancellationToken));

    [HttpPut("connections/{connectionId:guid?}")]
    [Authorize(Policy = AuthorizationPolicies.CalendarSyncManage)]
    public async Task<IActionResult> UpsertConnection(
        Guid? connectionId,
        [FromBody] UpsertCalendarConnectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpsertCalendarConnectionCommand(
                connectionId,
                request.Name,
                request.Provider,
                request.CalendarIdentifier,
                request.SecretReference,
                request.SyncDirection,
                request.IsActive),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("connections/{connectionId:guid}/sync")]
    [Authorize(Policy = AuthorizationPolicies.CalendarSyncManage)]
    public async Task<IActionResult> TriggerSync(Guid connectionId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new TriggerCalendarSyncCommand(connectionId), cancellationToken));

    public sealed record UpsertCalendarConnectionRequest(
        string Name,
        [property: JsonRequired] CalendarProviderType Provider,
        string CalendarIdentifier,
        string SecretReference,
        [property: JsonRequired] CalendarSyncDirection SyncDirection,
        [property: JsonRequired] bool IsActive);
}
