// <copyright file="IntegrationJob.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationJob : EntityBase
{
    public string ProviderKey { get; private set; } = null!;
    public string JobType { get; private set; } = null!;
    public string Direction { get; private set; } = null!;
    public string PayloadJson { get; private set; } = "{}";
    public string IdempotencyKey { get; private set; } = null!;
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public DateTime? StartedAtUtc { get; private set; }
    public DateTime? LastAttemptAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? DeadLetteredAtUtc { get; private set; }
    public DateTime? LeaseExpiresAtUtc { get; private set; }
    public string? LockedBy { get; private set; }
    public string Status { get; private set; } = IntegrationJobStatuses.Queued;
    public int AttemptCount { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public string? LastErrorCode { get; private set; }
    public string? LastErrorMessage { get; private set; }
    public string? ErrorClassification { get; private set; }
    public string? CorrelationId { get; private set; }
    public Guid? ReplayOfJobId { get; private set; }
    public bool IsReplay { get; private set; }
    public string? CancellationReason { get; private set; }

    private IntegrationJob() { }

    public IntegrationJob(Guid tenantId, string jobType, string direction, string payloadJson, DateTime scheduledAtUtc)
        : this(tenantId, jobType, jobType, direction, payloadJson, scheduledAtUtc, string.Empty, 3, null)
    {
    }

    public IntegrationJob(
        Guid tenantId,
        string providerKey,
        string jobType,
        string direction,
        string payloadJson,
        DateTime scheduledAtUtc,
        string idempotencyKey,
        int maxAttempts,
        Guid? replayOfJobId)
    {
        TenantId = tenantId;
        ProviderKey = Guard.AgainstNullOrWhiteSpace(providerKey).Trim();
        JobType = Guard.AgainstNullOrWhiteSpace(jobType).Trim();
        Direction = Guard.AgainstNullOrWhiteSpace(direction).Trim();
        PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson.Trim();
        ScheduledAtUtc = scheduledAtUtc;
        NextAttemptAtUtc = scheduledAtUtc;
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? $"{ProviderKey}:{JobType}:{Direction}:{Id:N}" : idempotencyKey.Trim();
        MaxAttempts = Math.Max(1, maxAttempts);
        ReplayOfJobId = replayOfJobId;
        IsReplay = replayOfJobId.HasValue;
    }

    public bool CanBeAcquired(DateTime nowUtc)
        => (Status is IntegrationJobStatuses.Queued or IntegrationJobStatuses.Retrying) &&
           ScheduledAtUtc <= nowUtc &&
           (NextAttemptAtUtc is null || NextAttemptAtUtc <= nowUtc);

    public bool TryAcquire(string workerId, DateTime nowUtc, TimeSpan leaseDuration)
    {
        if (!CanBeAcquired(nowUtc))
        {
            return false;
        }

        Status = IntegrationJobStatuses.Processing;
        LockedBy = Guard.AgainstNullOrWhiteSpace(workerId).Trim();
        LeaseExpiresAtUtc = nowUtc.Add(leaseDuration);
        StartedAtUtc ??= nowUtc;
        LastAttemptAtUtc = nowUtc;
        AttemptCount += 1;
        CorrelationId = $"{TenantId:N}-{Id:N}-{AttemptCount}";
        return true;
    }

    public void MarkCompleted(DateTime completedAtUtc)
    {
        Status = IntegrationJobStatuses.Completed;
        CompletedAtUtc = completedAtUtc;
        NextAttemptAtUtc = null;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        LastErrorCode = null;
        LastErrorMessage = null;
        ErrorClassification = null;
    }

    public void MarkRetry(DateTime nextAttemptAtUtc, string errorClassification, string errorCode, string sanitizedMessage)
    {
        Status = IntegrationJobStatuses.Retrying;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = errorClassification.Trim();
        LastErrorCode = errorCode.Trim();
        LastErrorMessage = sanitizedMessage.Trim();
    }

    public void MarkFailed(string errorClassification, string errorCode, string sanitizedMessage)
    {
        Status = IntegrationJobStatuses.Failed;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = errorClassification.Trim();
        LastErrorCode = errorCode.Trim();
        LastErrorMessage = sanitizedMessage.Trim();
    }

    public void MoveToDeadLetter(DateTime failedAtUtc, string errorClassification, string errorCode, string sanitizedMessage)
    {
        Status = IntegrationJobStatuses.DeadLettered;
        DeadLetteredAtUtc = failedAtUtc;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
        ErrorClassification = errorClassification.Trim();
        LastErrorCode = errorCode.Trim();
        LastErrorMessage = sanitizedMessage.Trim();
    }

    public void Cancel(DateTime cancelledAtUtc, string? reason)
    {
        if (Status is IntegrationJobStatuses.Completed or IntegrationJobStatuses.Canceled or IntegrationJobStatuses.DeadLettered)
        {
            return;
        }

        Status = Status == IntegrationJobStatuses.Processing
            ? IntegrationJobStatuses.CancelRequested
            : IntegrationJobStatuses.Canceled;
        CancelledAtUtc = cancelledAtUtc;
        CancellationReason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        if (Status == IntegrationJobStatuses.Canceled)
        {
            LockedBy = null;
            LeaseExpiresAtUtc = null;
        }
    }

    public void MarkCanceled(DateTime cancelledAtUtc)
    {
        Status = IntegrationJobStatuses.Canceled;
        CancelledAtUtc = cancelledAtUtc;
        LockedBy = null;
        LeaseExpiresAtUtc = null;
    }
}

public static class IntegrationJobStatuses
{
    public const string Queued = "queued";
    public const string Processing = "processing";
    public const string Retrying = "retrying";
    public const string Completed = "completed";
    public const string Failed = "failed";
    public const string DeadLettered = "dead-lettered";
    public const string Canceled = "canceled";
    public const string CancelRequested = "cancel-requested";
}
