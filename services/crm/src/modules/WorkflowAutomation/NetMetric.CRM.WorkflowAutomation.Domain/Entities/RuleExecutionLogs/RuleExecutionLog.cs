// <copyright file="RuleExecutionLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;

public sealed class RuleExecutionLog : AuditableEntity
{
    private RuleExecutionLog()
    {
    }

    public Guid RuleId { get; private set; }
    public int RuleVersion { get; private set; }
    public string RuleName { get; private set; } = string.Empty;
    public string TriggerType { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string Status { get; private set; } = WorkflowExecutionStatuses.Queued;
    public bool IsDryRun { get; private set; }
    public int AttemptNumber { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string LoopFingerprint { get; private set; } = string.Empty;
    public int LoopDepth { get; private set; }
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? DeadLetteredAtUtc { get; private set; }
    public DateTime? LeaseExpiresAtUtc { get; private set; }
    public string? LockedBy { get; private set; }
    public string? ErrorClassification { get; private set; }
    public string? ErrorCode { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string TriggerPayloadJson { get; private set; } = "{}";
    public string ConditionResultJson { get; private set; } = "{}";
    public string ActionResultJson { get; private set; } = "[]";
    public string PermissionSnapshotJson { get; private set; } = "[]";
    public Guid? RequestedByUserId { get; private set; }

    public static RuleExecutionLog Queue(
        Guid tenantId,
        Guid ruleId,
        int ruleVersion,
        string ruleName,
        string triggerType,
        string entityType,
        Guid? entityId,
        string idempotencyKey,
        string correlationId,
        string loopFingerprint,
        int loopDepth,
        int maxAttempts,
        DateTime scheduledAtUtc,
        string redactedPayloadJson,
        string permissionSnapshotJson,
        Guid? requestedByUserId)
    {
        return new RuleExecutionLog
        {
            TenantId = Guard.AgainstEmpty(tenantId),
            RuleId = Guard.AgainstEmpty(ruleId),
            RuleVersion = Math.Max(1, ruleVersion),
            RuleName = Guard.AgainstNullOrWhiteSpace(ruleName),
            TriggerType = Guard.AgainstNullOrWhiteSpace(triggerType),
            EntityType = Guard.AgainstNullOrWhiteSpace(entityType),
            EntityId = entityId,
            IdempotencyKey = Guard.AgainstNullOrWhiteSpace(idempotencyKey),
            CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId),
            LoopFingerprint = Guard.AgainstNullOrWhiteSpace(loopFingerprint),
            LoopDepth = Math.Max(0, loopDepth),
            MaxAttempts = Math.Clamp(maxAttempts, 1, 10),
            ScheduledAtUtc = scheduledAtUtc,
            NextAttemptAtUtc = scheduledAtUtc,
            TriggerPayloadJson = NormalizeJson(redactedPayloadJson, "{}"),
            PermissionSnapshotJson = NormalizeJson(permissionSnapshotJson, "[]"),
            RequestedByUserId = requestedByUserId
        };
    }

    public static RuleExecutionLog DryRun(
        Guid tenantId,
        Guid ruleId,
        int ruleVersion,
        string ruleName,
        string triggerType,
        string entityType,
        Guid? entityId,
        string correlationId,
        string loopFingerprint,
        string redactedPayloadJson,
        string conditionResultJson,
        string actionResultJson,
        Guid? requestedByUserId)
    {
        var log = Queue(
            tenantId,
            ruleId,
            ruleVersion,
            ruleName,
            triggerType,
            entityType,
            entityId,
            $"{tenantId:N}:{ruleId:N}:dry-run:{Guid.NewGuid():N}",
            correlationId,
            loopFingerprint,
            0,
            1,
            DateTime.UtcNow,
            redactedPayloadJson,
            "[]",
            requestedByUserId);
        log.IsDryRun = true;
        log.Status = WorkflowExecutionStatuses.Simulated;
        log.StartedAtUtc = DateTime.UtcNow;
        log.CompletedAtUtc = log.StartedAtUtc;
        log.ConditionResultJson = NormalizeJson(conditionResultJson, "{}");
        log.ActionResultJson = NormalizeJson(actionResultJson, "[]");
        return log;
    }

    public bool CanBeAcquired(DateTime nowUtc)
        => Status is WorkflowExecutionStatuses.Queued or WorkflowExecutionStatuses.Retrying &&
           ScheduledAtUtc <= nowUtc &&
           (NextAttemptAtUtc is null || NextAttemptAtUtc <= nowUtc);

    public bool TryAcquire(string workerId, DateTime nowUtc, TimeSpan leaseDuration)
    {
        if (!CanBeAcquired(nowUtc))
        {
            return false;
        }

        Status = WorkflowExecutionStatuses.Processing;
        LockedBy = Guard.AgainstNullOrWhiteSpace(workerId);
        LeaseExpiresAtUtc = nowUtc.Add(leaseDuration);
        StartedAtUtc ??= nowUtc;
        AttemptNumber += 1;
        return true;
    }

    public void MarkCompleted(string conditionResultJson, string actionResultJson, DateTime completedAtUtc)
    {
        Status = WorkflowExecutionStatuses.Completed;
        ConditionResultJson = NormalizeJson(conditionResultJson, "{}");
        ActionResultJson = NormalizeJson(actionResultJson, "[]");
        CompletedAtUtc = completedAtUtc;
        NextAttemptAtUtc = null;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = null;
        ErrorCode = null;
        ErrorMessage = null;
    }

    public void MarkSkipped(string status, string message, string conditionResultJson, DateTime completedAtUtc)
    {
        Status = Guard.AgainstNullOrWhiteSpace(status);
        ErrorMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        ConditionResultJson = NormalizeJson(conditionResultJson, "{}");
        CompletedAtUtc = completedAtUtc;
        NextAttemptAtUtc = null;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
    }

    public void MarkRetry(DateTime nextAttemptAtUtc, string errorClassification, string errorCode, string sanitizedMessage)
    {
        Status = WorkflowExecutionStatuses.Retrying;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = Guard.AgainstNullOrWhiteSpace(errorClassification);
        ErrorCode = Guard.AgainstNullOrWhiteSpace(errorCode);
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(sanitizedMessage);
    }

    public void MoveToDeadLetter(DateTime failedAtUtc, string errorClassification, string errorCode, string sanitizedMessage)
    {
        Status = WorkflowExecutionStatuses.DeadLettered;
        DeadLetteredAtUtc = failedAtUtc;
        CompletedAtUtc ??= failedAtUtc;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = Guard.AgainstNullOrWhiteSpace(errorClassification);
        ErrorCode = Guard.AgainstNullOrWhiteSpace(errorCode);
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(sanitizedMessage);
    }

    public void Requeue(DateTime scheduledAtUtc)
    {
        Status = WorkflowExecutionStatuses.Queued;
        ScheduledAtUtc = scheduledAtUtc;
        NextAttemptAtUtc = scheduledAtUtc;
        CompletedAtUtc = null;
        DeadLetteredAtUtc = null;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
    }

    private static string NormalizeJson(string? json, string fallback)
        => string.IsNullOrWhiteSpace(json) ? fallback : json.Trim();
}

public static class WorkflowExecutionStatuses
{
    public const string Queued = "queued";
    public const string Processing = "processing";
    public const string Retrying = "retrying";
    public const string Completed = "completed";
    public const string Skipped = "skipped";
    public const string Simulated = "simulated";
    public const string IdempotentSkip = "idempotent-skip";
    public const string LoopPrevented = "loop-prevented";
    public const string PermissionDenied = "permission-denied";
    public const string DeadLettered = "dead-lettered";
}
