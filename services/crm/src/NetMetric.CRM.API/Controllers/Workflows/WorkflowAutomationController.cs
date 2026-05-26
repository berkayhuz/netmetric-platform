// <copyright file="WorkflowAutomationController.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json.Serialization;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Approvals.Commands.CreateApprovalWorkflow;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Assignments.Commands.CreateAssignmentRule;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.ActivateAutomationRule;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.CreateAutomationRule;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DeactivateAutomationRule;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DryRunAutomationRule;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.EvaluateRules;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.RetryRuleExecution;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetAutomationRuleDetail;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetExecutionLogDetail;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetWorkflowWorkerStatus;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.ListAutomationRules;
using NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.ListExecutionLogs;

namespace NetMetric.CRM.API.Controllers.Workflows;

[ApiController]
[Route("api/workflows")]
[Authorize(Policy = AuthorizationPolicies.WorkflowRulesManage)]
public sealed class WorkflowAutomationController(IMediator mediator) : ControllerBase
{
    [HttpGet("tenants/{tenantId:guid}/rules")]
    public async Task<IActionResult> ListRules(
        Guid tenantId,
        [FromQuery] string? triggerType,
        [FromQuery] string? entityType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "updated",
        [FromQuery] string? sortDirection = "desc",
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListAutomationRulesQuery(tenantId, triggerType, entityType, isActive, page, pageSize, sortBy, sortDirection), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/rules/{ruleId:guid}")]
    public async Task<IActionResult> GetRuleDetail(Guid tenantId, Guid ruleId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetAutomationRuleDetailQuery(tenantId, ruleId), cancellationToken));

    [HttpPost("tenants/{tenantId:guid}/rules")]
    public async Task<IActionResult> CreateAutomationRule(Guid tenantId, [FromBody] CreateAutomationRuleRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(
            new CreateAutomationRuleCommand(
                request.Name,
                request.TriggerType,
                request.EntityType,
                request.TriggerDefinitionJson,
                request.ConditionDefinitionJson,
                request.ActionDefinitionJson,
                request.Description,
                request.Priority,
                request.MaxAttempts,
                request.TenantDailyExecutionLimit,
                request.LoopPreventionWindowSeconds,
                request.MaxLoopDepth,
                request.IsActive,
                request.NextRunAtUtc,
                request.ScheduleCron,
                request.ScheduleIntervalSeconds,
                request.TemplateKey),
            cancellationToken);

        return CreatedAtAction(nameof(GetRuleDetail), new { tenantId, ruleId = id }, new { id });
    }

    [HttpPost("tenants/{tenantId:guid}/rules/{ruleId:guid}/activate")]
    public async Task<IActionResult> ActivateRule(Guid tenantId, Guid ruleId, CancellationToken cancellationToken)
    {
        await mediator.Send(new ActivateAutomationRuleCommand(tenantId, ruleId), cancellationToken);
        return NoContent();
    }

    [HttpPost("tenants/{tenantId:guid}/rules/{ruleId:guid}/deactivate")]
    public async Task<IActionResult> DeactivateRule(Guid tenantId, Guid ruleId, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeactivateAutomationRuleCommand(tenantId, ruleId), cancellationToken);
        return NoContent();
    }

    [HttpPost("tenants/{tenantId:guid}/rules/evaluate")]
    public async Task<IActionResult> EvaluateRules(Guid tenantId, [FromBody] EvaluateRulesRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(
            new EvaluateRulesCommand(
                tenantId,
                request.TriggerType,
                request.EntityType,
                request.EntityId,
                request.PayloadJson,
                request.IdempotencyKey,
                request.CorrelationId,
                request.LoopDepth),
            cancellationToken));

    [HttpPost("tenants/{tenantId:guid}/rules/dry-run")]
    public async Task<IActionResult> DryRun(Guid tenantId, [FromBody] DryRunWorkflowRequest request, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(
            new DryRunAutomationRuleCommand(
                tenantId,
                request.TriggerType,
                request.EntityType,
                request.EntityId,
                request.PayloadJson,
                request.RuleId,
                request.CorrelationId),
            cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/executions")]
    [Authorize(Policy = AuthorizationPolicies.WorkflowRulesManage)]
    public async Task<IActionResult> ListExecutionLogs(
        Guid tenantId,
        [FromQuery] Guid? ruleId,
        [FromQuery] string? status,
        [FromQuery] bool? failedOnly,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        Ok(await mediator.Send(new ListExecutionLogsQuery(tenantId, ruleId, status, failedOnly, page, pageSize), cancellationToken));

    [HttpGet("tenants/{tenantId:guid}/executions/{executionLogId:guid}")]
    public async Task<IActionResult> GetExecutionLog(Guid tenantId, Guid executionLogId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetExecutionLogDetailQuery(tenantId, executionLogId), cancellationToken));

    [HttpPost("tenants/{tenantId:guid}/executions/{executionLogId:guid}/retry")]
    public async Task<IActionResult> RetryExecution(Guid tenantId, Guid executionLogId, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new RetryRuleExecutionCommand(tenantId, executionLogId), cancellationToken);
        return CreatedAtAction(nameof(GetExecutionLog), new { tenantId, executionLogId = id }, new { id });
    }

    [HttpGet("tenants/{tenantId:guid}/worker-status")]
    public async Task<IActionResult> GetWorkerStatus(Guid tenantId, CancellationToken cancellationToken) =>
        Ok(await mediator.Send(new GetWorkflowWorkerStatusQuery(tenantId), cancellationToken));

    [HttpPost("approvals")]
    [Authorize(Policy = AuthorizationPolicies.WorkflowApprovalsManage)]
    public async Task<IActionResult> CreateApprovalWorkflow([FromBody] CreateApprovalWorkflowRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateApprovalWorkflowCommand(request.Name, request.EntityType, request.RelatedEntityId, request.RoutingPolicyJson, request.EscalationPolicyJson, request.SlaPolicyJson), cancellationToken);
        return CreatedAtAction(nameof(CreateApprovalWorkflow), new { id }, new { id });
    }

    [HttpPost("assignment-rules")]
    [Authorize(Policy = AuthorizationPolicies.WorkflowAssignmentRulesManage)]
    public async Task<IActionResult> CreateAssignmentRule([FromBody] CreateAssignmentRuleRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateAssignmentRuleCommand(request.Name, request.EntityType, request.ConditionJson, request.AssigneeSelectorJson, request.FallbackAssigneeJson, request.Priority), cancellationToken);
        return CreatedAtAction(nameof(CreateAssignmentRule), new { id }, new { id });
    }

    public sealed record CreateApprovalWorkflowRequest(
        string Name,
        string EntityType,
        Guid? RelatedEntityId,
        string? RoutingPolicyJson,
        string? EscalationPolicyJson,
        string? SlaPolicyJson);

    public sealed record CreateAssignmentRuleRequest(
        string Name,
        string EntityType,
        string ConditionJson,
        string? AssigneeSelectorJson,
        string? FallbackAssigneeJson,
        [property: JsonRequired] int Priority = 100);

    public sealed record CreateAutomationRuleRequest(
        string Name,
        string TriggerType,
        string EntityType,
        string TriggerDefinitionJson,
        string ConditionDefinitionJson,
        string ActionDefinitionJson,
        string? Description,
        [property: JsonRequired] int Priority = 100,
        [property: JsonRequired] int MaxAttempts = 3,
        [property: JsonRequired] int TenantDailyExecutionLimit = 1000,
        [property: JsonRequired] int LoopPreventionWindowSeconds = 300,
        [property: JsonRequired] int MaxLoopDepth = 4,
        bool IsActive = false,
        DateTime? NextRunAtUtc = null,
        string? ScheduleCron = null,
        int? ScheduleIntervalSeconds = null,
        string? TemplateKey = null);

    public sealed record EvaluateRulesRequest(
        string TriggerType,
        string EntityType,
        Guid? EntityId,
        string PayloadJson,
        string? IdempotencyKey,
        string? CorrelationId,
        [property: JsonRequired] int LoopDepth = 0);

    public sealed record DryRunWorkflowRequest(
        string TriggerType,
        string EntityType,
        Guid? EntityId,
        string PayloadJson,
        Guid? RuleId,
        string? CorrelationId);
}
