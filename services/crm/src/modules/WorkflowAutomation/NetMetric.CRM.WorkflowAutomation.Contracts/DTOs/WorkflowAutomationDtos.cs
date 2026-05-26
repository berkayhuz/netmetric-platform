// <copyright file="WorkflowAutomationDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;

public sealed record WorkflowExecutionSummaryDto(
    Guid RuleId,
    string Name,
    int ExecutionsLast24Hours,
    int FailureCountLast24Hours);

public sealed record WorkflowRuleListItemDto(
    Guid Id,
    string Name,
    string TriggerType,
    string EntityType,
    int Version,
    bool IsActive,
    int Priority,
    DateTime? LastTriggeredAtUtc,
    string? LastExecutionStatus,
    DateTime? NextRunAtUtc);

public sealed record WorkflowRuleDetailDto(
    Guid Id,
    string Name,
    string? Description,
    string TriggerType,
    string EntityType,
    string TriggerDefinitionJson,
    string ConditionDefinitionJson,
    string ActionDefinitionJson,
    int Version,
    bool IsActive,
    int Priority,
    int MaxAttempts,
    int TenantDailyExecutionLimit,
    int LoopPreventionWindowSeconds,
    int MaxLoopDepth,
    DateTime? ActivatedAtUtc,
    DateTime? DeactivatedAtUtc,
    DateTime? LastTriggeredAtUtc,
    string? LastExecutionStatus,
    DateTime? NextRunAtUtc,
    string? ScheduleCron,
    int? ScheduleIntervalSeconds,
    IReadOnlyCollection<WorkflowRuleVersionDto> Versions);

public sealed record WorkflowRuleVersionDto(
    Guid Id,
    int Version,
    string ChangeReason,
    string? ChangedBy,
    DateTime CreatedAt);

public sealed record WorkflowExecutionLogListItemDto(
    Guid Id,
    Guid RuleId,
    string RuleName,
    int RuleVersion,
    string TriggerType,
    string EntityType,
    Guid? EntityId,
    string Status,
    bool IsDryRun,
    int AttemptNumber,
    int MaxAttempts,
    DateTime ScheduledAtUtc,
    DateTime? NextAttemptAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? DeadLetteredAtUtc,
    string? ErrorClassification,
    string? ErrorCode,
    string? ErrorMessage,
    string CorrelationId);

public sealed record WorkflowExecutionLogDetailDto(
    Guid Id,
    Guid RuleId,
    string RuleName,
    int RuleVersion,
    string TriggerType,
    string EntityType,
    Guid? EntityId,
    string Status,
    bool IsDryRun,
    int AttemptNumber,
    int MaxAttempts,
    string IdempotencyKey,
    string CorrelationId,
    string LoopFingerprint,
    int LoopDepth,
    DateTime ScheduledAtUtc,
    DateTime? NextAttemptAtUtc,
    DateTime? StartedAtUtc,
    DateTime? CompletedAtUtc,
    DateTime? DeadLetteredAtUtc,
    string? ErrorClassification,
    string? ErrorCode,
    string? ErrorMessage,
    string TriggerPayloadJson,
    string ConditionResultJson,
    string ActionResultJson,
    IReadOnlyCollection<WorkflowWebhookDeliveryDto> WebhookDeliveries);

public sealed record WorkflowWebhookDeliveryDto(
    Guid Id,
    string EventKey,
    string TargetUrl,
    string Status,
    int AttemptNumber,
    int MaxAttempts,
    int? HttpStatusCode,
    string? ResponseSnippet,
    string? ErrorMessage,
    DateTime? DeliveredAtUtc,
    DateTime? NextAttemptAtUtc,
    string CorrelationId);

public sealed record WorkflowRuleSimulationDto(
    Guid RuleId,
    string RuleName,
    int RuleVersion,
    bool TriggerMatched,
    bool ConditionsMatched,
    bool LoopPrevented,
    string ConditionResultJson,
    IReadOnlyCollection<WorkflowActionSimulationDto> Actions);

public sealed record WorkflowActionSimulationDto(
    string ActionType,
    string Status,
    string Message,
    bool RequiresPermission,
    string? RequiredPermission);

public sealed record WorkflowRuleExecutionResultDto(
    Guid? ExecutionId,
    bool DryRun,
    int MatchedRules,
    int ExecutedActions,
    string Status,
    IReadOnlyCollection<WorkflowRuleSimulationDto> Rules);

public sealed record WorkflowWorkerStatusDto(
    bool IsEnabled,
    int DueExecutions,
    int ProcessingExecutions,
    int RetryingExecutions,
    int DeadLetteredExecutions,
    DateTime CheckedAtUtc);
