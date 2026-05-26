// <copyright file="JourneyStepExecution.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;

public sealed class JourneyStepExecution : AuditableEntity
{
    private JourneyStepExecution() { }

    public JourneyStepExecution(Guid journeyId, string stepKey, string emailHash, DateTime scheduledAtUtc, string idempotencyKey, int maxAttempts = 3)
    {
        JourneyId = Guard.AgainstEmpty(journeyId);
        StepKey = Guard.AgainstNullOrWhiteSpace(stepKey);
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        ScheduledAtUtc = scheduledAtUtc;
        NextAttemptAtUtc = scheduledAtUtc;
        IdempotencyKey = Guard.AgainstNullOrWhiteSpace(idempotencyKey);
        MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
    }

    public Guid JourneyId { get; private set; }
    public string StepKey { get; private set; } = string.Empty;
    public string EmailHash { get; private set; } = string.Empty;
    public string Status { get; private set; } = JourneyStepStatuses.Queued;
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public int AttemptNumber { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string? ErrorMessage { get; private set; }

    public bool CanRun(DateTime nowUtc)
        => Status is JourneyStepStatuses.Queued or JourneyStepStatuses.Retrying &&
           ScheduledAtUtc <= nowUtc &&
           (NextAttemptAtUtc is null || NextAttemptAtUtc <= nowUtc);

    public void MarkProcessing()
    {
        Status = JourneyStepStatuses.Processing;
        AttemptNumber += 1;
    }

    public void MarkCompleted()
    {
        Status = JourneyStepStatuses.Completed;
        NextAttemptAtUtc = null;
        ErrorMessage = null;
    }

    public void MarkRetry(DateTime nextAttemptAtUtc, string message)
    {
        Status = JourneyStepStatuses.Retrying;
        NextAttemptAtUtc = nextAttemptAtUtc;
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(message);
    }

    public void MarkFailed(string message)
    {
        Status = JourneyStepStatuses.Failed;
        NextAttemptAtUtc = null;
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(message);
    }
}

public static class JourneyStepStatuses
{
    public const string Queued = "queued";
    public const string Processing = "processing";
    public const string Retrying = "retrying";
    public const string Completed = "completed";
    public const string Failed = "failed";
}
