// <copyright file="MarketingEmailDelivery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;

public sealed class MarketingEmailDelivery : AuditableEntity
{
    private MarketingEmailDelivery() { }

    public MarketingEmailDelivery(Guid campaignId, Guid? journeyId, Guid? journeyStepExecutionId, string emailHash, string idempotencyKey, DateTime scheduledAtUtc, string correlationId, int maxAttempts)
    {
        CampaignId = campaignId;
        JourneyId = journeyId;
        JourneyStepExecutionId = journeyStepExecutionId;
        EmailHash = Guard.AgainstNullOrWhiteSpace(emailHash);
        IdempotencyKey = Guard.AgainstNullOrWhiteSpace(idempotencyKey);
        ScheduledAtUtc = scheduledAtUtc;
        NextAttemptAtUtc = scheduledAtUtc;
        CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId);
        MaxAttempts = Math.Clamp(maxAttempts, 1, 10);
    }

    public Guid CampaignId { get; private set; }
    public Guid? JourneyId { get; private set; }
    public Guid? JourneyStepExecutionId { get; private set; }
    public string EmailHash { get; private set; } = string.Empty;
    public string Status { get; private set; } = MarketingDeliveryStatuses.Queued;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public int AttemptNumber { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public DateTime ScheduledAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public DateTime? SentAtUtc { get; private set; }
    public DateTime? FailedAtUtc { get; private set; }
    public string? FailureCode { get; private set; }
    public string? FailureMessage { get; private set; }

    public bool CanRun(DateTime nowUtc)
        => Status is MarketingDeliveryStatuses.Queued or MarketingDeliveryStatuses.Retrying &&
           ScheduledAtUtc <= nowUtc &&
           (NextAttemptAtUtc is null || NextAttemptAtUtc <= nowUtc);

    public void MarkAttempt()
    {
        Status = MarketingDeliveryStatuses.Processing;
        AttemptNumber += 1;
    }

    public void MarkSent(DateTime sentAtUtc)
    {
        Status = MarketingDeliveryStatuses.Sent;
        SentAtUtc = sentAtUtc;
        NextAttemptAtUtc = null;
        FailureCode = null;
        FailureMessage = null;
    }

    public void MarkRetry(DateTime nextAttemptAtUtc, string code, string message)
    {
        Status = MarketingDeliveryStatuses.Retrying;
        NextAttemptAtUtc = nextAttemptAtUtc;
        FailureCode = Guard.AgainstNullOrWhiteSpace(code);
        FailureMessage = Guard.AgainstNullOrWhiteSpace(message);
    }

    public void MarkFailed(DateTime failedAtUtc, string code, string message)
    {
        Status = MarketingDeliveryStatuses.Failed;
        FailedAtUtc = failedAtUtc;
        NextAttemptAtUtc = null;
        FailureCode = Guard.AgainstNullOrWhiteSpace(code);
        FailureMessage = Guard.AgainstNullOrWhiteSpace(message);
    }

    public void MarkSuppressed(string reason)
    {
        Status = MarketingDeliveryStatuses.Suppressed;
        FailureCode = "suppressed";
        FailureMessage = Guard.AgainstNullOrWhiteSpace(reason);
        NextAttemptAtUtc = null;
    }
}

public static class MarketingDeliveryStatuses
{
    public const string Queued = "queued";
    public const string Processing = "processing";
    public const string Retrying = "retrying";
    public const string Sent = "sent";
    public const string Suppressed = "suppressed";
    public const string Failed = "failed";
}
