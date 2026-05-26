// <copyright file="SupportInboxController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.CreateSupportInboxConnection;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Connections.UpdateSupportInboxConnection;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.CreateSupportInboxRule;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.UpdateSupportInboxRule;
using NetMetric.CRM.SupportInboxIntegration.Application.Commands.Sync.TriggerSupportInboxSync;
using NetMetric.CRM.SupportInboxIntegration.Application.Queries.Connections.GetSupportInboxConnections;
using NetMetric.CRM.SupportInboxIntegration.Application.Queries.Messages.GetSupportInboxMessages;
using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;

namespace NetMetric.CRM.API.Controllers.Supports;

[ApiController]
[Route("api/support-inbox")]
[Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsRead)]
public sealed class SupportInboxController(IMediator mediator) : ControllerBase
{
    [HttpGet("connections")]
    public async Task<IActionResult> GetConnections(CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetSupportInboxConnectionsQuery(), cancellationToken));

    [HttpPost("connections")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsManage)]
    public async Task<IActionResult> CreateConnection([FromBody] CreateConnectionRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new CreateSupportInboxConnectionCommand(request.Name, request.Provider, request.EmailAddress, request.Host, request.Port, request.Username, request.SecretReference, request.UseSsl), cancellationToken));

    [HttpPut("connections/{connectionId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsManage)]
    public async Task<IActionResult> UpdateConnection(Guid connectionId, [FromBody] UpdateConnectionRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new UpdateSupportInboxConnectionCommand(connectionId, request.Name, request.Host, request.Port, request.Username, request.SecretReference, request.UseSsl, request.IsActive), cancellationToken));

    [HttpPost("connections/{connectionId:guid}/sync")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsManage)]
    public async Task<IActionResult> TriggerSync(Guid connectionId, [FromBody] TriggerSyncRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new TriggerSupportInboxSyncCommand(connectionId, request.DryRun), cancellationToken);
        return Accepted();
    }

    [HttpGet("messages")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxMessagesRead)]
    public async Task<IActionResult> GetMessages(Guid? connectionId, bool? linkedToTicket, int page = 1, int pageSize = 20, CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new GetSupportInboxMessagesQuery(connectionId, linkedToTicket, page, pageSize), cancellationToken));

    [HttpPost("rules")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsManage)]
    public async Task<IActionResult> CreateRule([FromBody] RuleCreateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new CreateSupportInboxRuleCommand(request.ConnectionId, request.Name, request.MatchSender, request.MatchSubjectContains, request.AssignToQueueId, request.TicketCategoryId, request.SlaPolicyId, request.AutoCreateTicket), cancellationToken);
        return NoContent();
    }

    [HttpPut("rules/{ruleId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.SupportInboxConnectionsManage)]
    public async Task<IActionResult> UpdateRule(Guid ruleId, [FromBody] RuleUpdateRequest request, CancellationToken cancellationToken)
    {
        await mediator.Send(new UpdateSupportInboxRuleCommand(ruleId, request.Name, request.MatchSender, request.MatchSubjectContains, request.AssignToQueueId, request.TicketCategoryId, request.SlaPolicyId, request.AutoCreateTicket, request.IsActive), cancellationToken);
        return NoContent();
    }

    public sealed record CreateConnectionRequest(string Name, [property: JsonRequired] SupportInboxProviderType Provider, string EmailAddress, string Host, [property: JsonRequired] int Port, string Username, string SecretReference, [property: JsonRequired] bool UseSsl);

    public sealed record UpdateConnectionRequest(string Name, string Host, [property: JsonRequired] int Port, string Username, string SecretReference, [property: JsonRequired] bool UseSsl, [property: JsonRequired] bool IsActive);

    public sealed record TriggerSyncRequest([property: JsonRequired] bool DryRun);

    public sealed record RuleCreateRequest([property: JsonRequired] Guid ConnectionId, string Name, string? MatchSender, string? MatchSubjectContains, Guid? AssignToQueueId, Guid? TicketCategoryId, Guid? SlaPolicyId, [property: JsonRequired] bool AutoCreateTicket);

    public sealed record RuleUpdateRequest(string Name, string? MatchSender, string? MatchSubjectContains, Guid? AssignToQueueId, Guid? TicketCategoryId, Guid? SlaPolicyId, [property: JsonRequired] bool AutoCreateTicket, [property: JsonRequired] bool IsActive);
}
