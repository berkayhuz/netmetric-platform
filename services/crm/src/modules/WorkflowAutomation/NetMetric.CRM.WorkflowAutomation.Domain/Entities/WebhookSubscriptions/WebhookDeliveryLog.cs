// <copyright file="WebhookDeliveryLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.WorkflowAutomation.Domain.Entities.WebhookSubscriptions;

public sealed class WebhookDeliveryLog : AuditableEntity
{
    private WebhookDeliveryLog()
    {
    }

    public Guid? SubscriptionId { get; private set; }
    public Guid RuleId { get; private set; }
    public Guid ExecutionLogId { get; private set; }
    public string EventKey { get; private set; } = string.Empty;
    public string TargetUrl { get; private set; } = string.Empty;
    public string Status { get; private set; } = WebhookDeliveryStatuses.Pending;
    public int AttemptNumber { get; private set; }
    public int MaxAttempts { get; private set; } = 3;
    public int? HttpStatusCode { get; private set; }
    public string RequestPayloadJson { get; private set; } = "{}";
    public string? ResponseSnippet { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string SignatureHeader { get; private set; } = string.Empty;
    public DateTime? DeliveredAtUtc { get; private set; }
    public DateTime? NextAttemptAtUtc { get; private set; }
    public string CorrelationId { get; private set; } = string.Empty;

    public static WebhookDeliveryLog Create(
        Guid tenantId,
        Guid ruleId,
        Guid executionLogId,
        string eventKey,
        string targetUrl,
        string redactedPayloadJson,
        string signatureHeader,
        string correlationId,
        Guid? subscriptionId = null,
        int maxAttempts = 3)
    {
        return new WebhookDeliveryLog
        {
            TenantId = Guard.AgainstEmpty(tenantId),
            RuleId = Guard.AgainstEmpty(ruleId),
            ExecutionLogId = Guard.AgainstEmpty(executionLogId),
            SubscriptionId = subscriptionId,
            EventKey = Guard.AgainstNullOrWhiteSpace(eventKey),
            TargetUrl = Guard.AgainstNullOrWhiteSpace(targetUrl),
            RequestPayloadJson = string.IsNullOrWhiteSpace(redactedPayloadJson) ? "{}" : redactedPayloadJson.Trim(),
            SignatureHeader = Guard.AgainstNullOrWhiteSpace(signatureHeader),
            CorrelationId = Guard.AgainstNullOrWhiteSpace(correlationId),
            MaxAttempts = Math.Clamp(maxAttempts, 1, 10)
        };
    }

    public static WebhookDeliveryLog CreateBlockedAttempt(
        Guid tenantId,
        Guid ruleId,
        Guid executionLogId,
        string eventKey,
        string safeTarget,
        string failureReason,
        string correlationId,
        int maxAttempts = 3)
    {
        var delivery = Create(
            tenantId,
            ruleId,
            executionLogId,
            eventKey,
            safeTarget,
            "{}",
            "signature=not-generated",
            correlationId,
            maxAttempts: maxAttempts);

        delivery.MarkFailure(failureReason, nextAttemptAtUtc: null);
        return delivery;
    }

    public void MarkAttempt(int httpStatusCode, string? responseSnippet, DateTime attemptedAtUtc)
    {
        AttemptNumber += 1;
        HttpStatusCode = httpStatusCode;
        ResponseSnippet = string.IsNullOrWhiteSpace(responseSnippet) ? null : responseSnippet.Trim();
        DeliveredAtUtc = attemptedAtUtc;
        Status = httpStatusCode is >= 200 and <= 299
            ? WebhookDeliveryStatuses.Delivered
            : WebhookDeliveryStatuses.Retrying;
    }

    public void MarkFailure(string errorMessage, DateTime? nextAttemptAtUtc)
    {
        AttemptNumber += 1;
        ErrorMessage = Guard.AgainstNullOrWhiteSpace(errorMessage);
        NextAttemptAtUtc = nextAttemptAtUtc;
        Status = nextAttemptAtUtc.HasValue ? WebhookDeliveryStatuses.Retrying : WebhookDeliveryStatuses.Failed;
    }
}

public static class WebhookDeliveryStatuses
{
    public const string Pending = "pending";
    public const string Delivered = "delivered";
    public const string Retrying = "retrying";
    public const string Failed = "failed";
}
