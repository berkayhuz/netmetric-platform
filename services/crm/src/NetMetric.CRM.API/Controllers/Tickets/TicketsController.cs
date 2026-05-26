// <copyright file="TicketsController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Application.Queries.Tickets;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CRM.TicketManagement.Contracts.Requests;
using NetMetric.CRM.Types;
using NetMetric.Pagination;

namespace NetMetric.CRM.API.Controllers.Tickets;

[ApiController]
[Route("api/tickets")]
[Authorize(Policy = AuthorizationPolicies.TicketsRead)]
public sealed class TicketsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TicketListItemDto>>> Get(
        [FromQuery] string? search,
        [FromQuery] TicketStatusType? status,
        [FromQuery] PriorityType? priority,
        [FromQuery] Guid? assignedUserId,
        [FromQuery] Guid? customerId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(
            new GetTicketsQuery(search, status, priority, assignedUserId, customerId, isActive, page, pageSize),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("{ticketId:guid}")]
    public async Task<ActionResult<TicketDetailDto>> GetById(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketByIdQuery(ticketId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.TicketsManage)]
    public async Task<ActionResult<TicketDetailDto>> Create([FromBody] TicketUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateTicketCommand(
                request.Subject,
                request.Description,
                request.TicketType,
                request.Channel,
                request.Priority,
                request.AssignedUserId,
                request.CustomerId,
                request.ContactId,
                request.TicketCategoryId,
                request.SlaPolicyId,
                request.FirstResponseDueAt,
                request.ResolveDueAt,
                request.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { ticketId = result.Id }, result);
    }

    [HttpPut("{ticketId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketsManage)]
    public async Task<ActionResult<TicketDetailDto>> Update(Guid ticketId, [FromBody] TicketUpsertRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateTicketCommand(
                ticketId,
                request.Subject,
                request.Description,
                request.TicketType,
                request.Channel,
                request.Priority,
                request.AssignedUserId,
                request.CustomerId,
                request.ContactId,
                request.TicketCategoryId,
                request.SlaPolicyId,
                request.FirstResponseDueAt,
                request.ResolveDueAt,
                request.Notes,
                request.RowVersion),
            cancellationToken);

        return Ok(result);
    }

    [HttpDelete("{ticketId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketsManage)]
    public async Task<IActionResult> Delete(Guid ticketId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteTicketCommand(ticketId), cancellationToken);
        return NoContent();
    }
}
