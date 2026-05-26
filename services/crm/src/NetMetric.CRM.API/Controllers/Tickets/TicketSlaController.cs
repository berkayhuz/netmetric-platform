// <copyright file="TicketSlaController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.Escalations;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.TicketSla;
using NetMetric.CRM.TicketSlaManagement.Application.Queries.Escalations;
using NetMetric.CRM.TicketSlaManagement.Application.Queries.SlaPolicies;
using NetMetric.CRM.TicketSlaManagement.Application.Queries.TicketSla;
using NetMetric.CRM.TicketSlaManagement.Contracts.Requests;

namespace NetMetric.CRM.API.Controllers.Tickets;

[ApiController]
[Route("api/ticket-sla")]
[Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesRead)]
public sealed class TicketSlaController(IMediator mediator) : ControllerBase
{
    [HttpGet("policies")]
    public async Task<IActionResult> GetPolicies(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSlaPoliciesQuery(), cancellationToken));

    [HttpPost("policies")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> CreatePolicy([FromBody] SlaPolicyUpsertRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(CreateSlaPolicyCommand.FromRequest(request), cancellationToken);
        return CreatedAtAction(nameof(GetPolicies), new { id }, new { id });
    }

    [HttpPut("policies/{policyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> UpdatePolicy(Guid policyId, [FromBody] SlaPolicyUpsertRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(UpdateSlaPolicyCommand.FromRequest(policyId, request), cancellationToken);
        return NoContent();
    }

    [HttpDelete("policies/{policyId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> DeletePolicy(Guid policyId, CancellationToken cancellationToken)
    {
        await mediator.Send(new SoftDeleteSlaPolicyCommand(policyId), cancellationToken);
        return NoContent();
    }

    [HttpGet("policies/{policyId:guid}/escalation-rules")]
    public async Task<IActionResult> GetEscalationRules(Guid policyId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSlaEscalationRulesQuery(policyId), cancellationToken));

    [HttpPost("escalation-rules")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> CreateEscalationRule([FromBody] SlaEscalationRuleUpsertRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(CreateSlaEscalationRuleCommand.FromRequest(request), cancellationToken);
        return CreatedAtAction(nameof(GetEscalationRules), new { policyId = request.SlaPolicyId }, new { id });
    }

    [HttpPut("escalation-rules/{ruleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> UpdateEscalationRule(Guid ruleId, [FromBody] SlaEscalationRuleUpsertRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(UpdateSlaEscalationRuleCommand.FromRequest(ruleId, request), cancellationToken);
        return NoContent();
    }

    [HttpGet("tickets/{ticketId:guid}/workspace")]
    public async Task<IActionResult> GetTicketWorkspace(Guid ticketId, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTicketSlaWorkspaceQuery(ticketId), cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("tickets/{ticketId:guid}/escalation-runs")]
    public async Task<IActionResult> GetTicketEscalationRuns(Guid ticketId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetTicketEscalationRunsQuery(ticketId), cancellationToken));

    [HttpPost("tickets/attach")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> AttachToTicket([FromBody] AttachSlaToTicketRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(AttachSlaToTicketCommand.FromRequest(request), cancellationToken);
        return NoContent();
    }

    [HttpPost("tickets/first-response")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> MarkFirstResponse([FromBody] MarkFirstResponseRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(MarkFirstResponseCommand.FromRequest(request), cancellationToken);
        return NoContent();
    }

    [HttpPost("tickets/resolved")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> MarkResolved([FromBody] MarkResolvedRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(MarkResolvedCommand.FromRequest(request), cancellationToken);
        return NoContent();
    }

    [HttpPost("escalations/run-due")]
    [Authorize(Policy = AuthorizationPolicies.TicketSlaPoliciesManage)]
    public async Task<IActionResult> RunDueEscalations([FromBody] RunDueEscalationsRequest request, CancellationToken cancellationToken) =>
        Ok(new { processed = await mediator.Send(new RunDueEscalationsCommand(request.UtcNow), cancellationToken) });

    public sealed record RunDueEscalationsRequest([property: JsonRequired] DateTime UtcNow);
}
