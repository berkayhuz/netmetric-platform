// <copyright file="WebhookDeliveryAttempt.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class WebhookDeliveryAttempt : EntityBase
{
    private WebhookDeliveryAttempt() { }

    public Guid WebhookSubscriptionId { get; private set; }
    public string EventId { get; private set; } = null!;
    public string EventType { get; private set; } = null!;
    public string Status { get; private set; } = WebhookDeliveryAttemptStatuses.Pending;
    public int AttemptCount { get; private set; }
    public int? HttpStatusCode { get; private set; }
    public string? LastErrorSummary { get; private set; }
    public DateTime TriggeredAtUtc { get; private set; }
    public DateTime? LastAttemptAtUtc { get; private set; }

    public WebhookDeliveryAttempt(Guid tenantId, Guid webhookSubscriptionId, string eventId, string eventType, DateTime triggeredAtUtc)
    {
        TenantId = tenantId;
        WebhookSubscriptionId = webhookSubscriptionId;
        EventId = Guard.AgainstNullOrWhiteSpace(eventId).Trim();
        EventType = Guard.AgainstNullOrWhiteSpace(eventType).Trim();
        TriggeredAtUtc = triggeredAtUtc;
    }

    public void MarkDelivered(int httpStatusCode, DateTime attemptedAtUtc)
    {
        Status = WebhookDeliveryAttemptStatuses.Delivered;
        HttpStatusCode = httpStatusCode;
        AttemptCount += 1;
        LastAttemptAtUtc = attemptedAtUtc;
        LastErrorSummary = null;
    }

    public void MarkFailed(int? httpStatusCode, string? safeSummary, DateTime attemptedAtUtc)
    {
        Status = WebhookDeliveryAttemptStatuses.Failed;
        HttpStatusCode = httpStatusCode;
        AttemptCount += 1;
        LastAttemptAtUtc = attemptedAtUtc;
        LastErrorSummary = string.IsNullOrWhiteSpace(safeSummary) ? null : safeSummary.Trim();
    }
}

public static class WebhookDeliveryAttemptStatuses
{
    public const string Pending = "pending";
    public const string Delivered = "delivered";
    public const string Failed = "failed";
    public const string Retrying = "retrying";
    public const string DeadLettered = "dead-lettered";
}
