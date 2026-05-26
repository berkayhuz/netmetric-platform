// <copyright file="TicketWorkflowController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketOwner;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Assignments.AssignTicketToQueue;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.CreateTicketQueue;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.SoftDeleteTicketQueue;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.UpdateTicketQueue;
using NetMetric.CRM.TicketWorkflowManagement.Application.Commands.StatusHistory.RecordTicketStatusChange;
using NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketAssignmentHistory;
using NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketQueues;
using NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketStatusHistory;
using NetMetric.CRM.TicketWorkflowManagement.Domain.Enums;

namespace NetMetric.CRM.API.Controllers.Tickets;

[ApiController]
[Route("api/ticket-workflow")]
[Authorize(Policy = AuthorizationPolicies.TicketQueuesRead)]
public sealed class TicketWorkflowController(IMediator mediator) : ControllerBase
{
    [HttpGet("queues")]
    public async Task<IActionResult> GetQueues(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTicketQueuesQuery(), cancellationToken));

    [HttpPost("queues")]
    [Authorize(Policy = AuthorizationPolicies.TicketQueuesManage)]
    public async Task<IActionResult> CreateQueue([FromBody] UpsertTicketQueueRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateTicketQueueCommand(request.Code, request.Name, request.Description, request.AssignmentStrategy, request.IsDefault), cancellationToken);
        return CreatedAtAction(nameof(GetQueues), new { id }, new { id });
    }

    [HttpPut("queues/{queueId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketQueuesManage)]
    public async Task<IActionResult> UpdateQueue(Guid queueId, [FromBody] UpdateTicketQueueRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateTicketQueueCommand(queueId, request.Name, request.Description, request.AssignmentStrategy, request.IsDefault), cancellationToken);
        return NoContent();
    }

    [HttpDelete("queues/{queueId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketQueuesManage)]
    public async Task<IActionResult> DeleteQueue(Guid queueId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteTicketQueueCommand(queueId), cancellationToken);
        return NoContent();
    }

    [HttpGet("tickets/{ticketId:guid}/assignments")]
    [Authorize(Policy = AuthorizationPolicies.TicketAssignmentsRead)]
    public async Task<IActionResult> GetAssignmentHistory(Guid ticketId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTicketAssignmentHistoryQuery(ticketId), cancellationToken));

    [HttpGet("tickets/{ticketId:guid}/status-history")]
    [Authorize(Policy = AuthorizationPolicies.TicketStatusHistoryRead)]
    public async Task<IActionResult> GetStatusHistory(Guid ticketId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTicketStatusHistoryQuery(ticketId), cancellationToken));

    [HttpPatch("tickets/{ticketId:guid}/queue")]
    [Authorize(Policy = AuthorizationPolicies.TicketQueuesManage)]
    public async Task<IActionResult> AssignToQueue(Guid ticketId, [FromBody] AssignTicketToQueueRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignTicketToQueueCommand(ticketId, request.PreviousQueueId, request.NewQueueId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPatch("tickets/{ticketId:guid}/owner")]
    [Authorize(Policy = AuthorizationPolicies.TicketsManage)]
    public async Task<IActionResult> AssignOwner(Guid ticketId, [FromBody] AssignTicketOwnerRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new AssignTicketOwnerCommand(ticketId, request.PreviousOwnerUserId, request.NewOwnerUserId, request.QueueId, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("tickets/{ticketId:guid}/status-history")]
    [Authorize(Policy = AuthorizationPolicies.TicketsManage)]
    public async Task<IActionResult> RecordStatusChange(Guid ticketId, [FromBody] RecordTicketStatusChangeRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new RecordTicketStatusChangeCommand(ticketId, request.PreviousStatus, request.NewStatus, request.Note), cancellationToken);
        return NoContent();
    }

    public sealed record UpsertTicketQueueRequest(string Code, string Name, string? Description, [property: JsonRequired] TicketQueueAssignmentStrategy AssignmentStrategy, [property: JsonRequired] bool IsDefault);

    public sealed record UpdateTicketQueueRequest(string Name, string? Description, [property: JsonRequired] TicketQueueAssignmentStrategy AssignmentStrategy, [property: JsonRequired] bool IsDefault);

    public sealed record AssignTicketToQueueRequest(Guid? PreviousQueueId, [property: JsonRequired] Guid NewQueueId, string? Reason);

    public sealed record AssignTicketOwnerRequest(Guid? PreviousOwnerUserId, [property: JsonRequired] Guid NewOwnerUserId, Guid? QueueId, string? Reason);

    public sealed record RecordTicketStatusChangeRequest(string PreviousStatus, string NewStatus, string? Note);
}
