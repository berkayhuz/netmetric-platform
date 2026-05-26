// <copyright file="WorkflowAutomationAbstractions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;

namespace NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;

public interface IWorkflowRuleEngine
{
    Task<WorkflowRuleExecutionResultDto> ExecuteAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken);
    Task<WorkflowRuleExecutionResultDto> DryRunAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken);
    Task<WorkflowRuleExecutionResultDto> ExecuteQueuedAsync(Guid executionLogId, CancellationToken cancellationToken);
}

public interface IWorkflowTriggerEvaluator
{
    bool IsMatch(AutomationRule rule, WorkflowRuleExecutionRequest request);
}

public interface IWorkflowConditionEvaluator
{
    WorkflowConditionEvaluationResult Evaluate(string conditionDefinitionJson, string payloadJson);
}

public interface IWorkflowActionDispatcher
{
    Task<WorkflowActionDispatchResult> DispatchAsync(WorkflowActionDispatchContext context, CancellationToken cancellationToken);
}

public interface IWorkflowActionPermissionGuard
{
    Task AuthorizeAsync(WorkflowActionPermissionContext context, CancellationToken cancellationToken);
}

public interface IWorkflowPayloadRedactor
{
    string RedactJson(string? json);
    string RedactText(string? text, int maxLength = 1000);
}

public interface IWorkflowExecutionProcessor
{
    Task<int> ProcessDueExecutionsAsync(CancellationToken cancellationToken);
}

public interface IWorkflowExecutionProcessingState
{
    bool IsEnabled { get; }
}

public sealed record WorkflowRuleExecutionRequest(
    Guid TenantId,
    string TriggerType,
    string EntityType,
    Guid? EntityId,
    string PayloadJson,
    string? IdempotencyKey = null,
    string? CorrelationId = null,
    int LoopDepth = 0,
    Guid? RuleId = null,
    DateTime? ScheduledAtUtc = null);

public sealed record WorkflowConditionEvaluationResult(
    bool Matched,
    string ResultJson,
    IReadOnlyCollection<string> FailureReasons);

public sealed record WorkflowActionDispatchContext(
    AutomationRule Rule,
    Guid TenantId,
    Guid ExecutionLogId,
    string TriggerType,
    string EntityType,
    Guid? EntityId,
    string PayloadJson,
    string ActionDefinitionJson,
    string CorrelationId,
    bool DryRun,
    string PermissionSnapshotJson);

public sealed record WorkflowActionDispatchResult(
    int ExecutedActions,
    string ActionResultJson,
    IReadOnlyCollection<WorkflowActionSimulationDto> Actions);

public sealed record WorkflowActionPermissionContext(
    string ActionType,
    string? RequiredPermission,
    string PermissionSnapshotJson,
    bool DryRun);
