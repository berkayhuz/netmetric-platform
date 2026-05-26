// <copyright file="IntegrationWebhookDelivery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationWebhookDelivery : EntityBase
{
    public string ProviderKey { get; private set; } = null!;
    public string EventId { get; private set; } = null!;
    public string SignatureHash { get; private set; } = null!;
    public string PayloadHash { get; private set; } = null!;
    public string Status { get; private set; } = IntegrationWebhookDeliveryStatuses.Accepted;
    public DateTime ReceivedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }
    public string? FailureReason { get; private set; }

    private IntegrationWebhookDelivery() { }

    public IntegrationWebhookDelivery(
        Guid tenantId,
        string providerKey,
        string eventId,
        string signatureHash,
        string payloadHash,
        DateTime receivedAtUtc)
    {
        TenantId = tenantId;
        ProviderKey = Guard.AgainstNullOrWhiteSpace(providerKey).Trim();
        EventId = Guard.AgainstNullOrWhiteSpace(eventId).Trim();
        SignatureHash = Guard.AgainstNullOrWhiteSpace(signatureHash).Trim();
        PayloadHash = Guard.AgainstNullOrWhiteSpace(payloadHash).Trim();
        ReceivedAtUtc = receivedAtUtc;
    }

    public void MarkProcessed(DateTime processedAtUtc)
    {
        Status = IntegrationWebhookDeliveryStatuses.Processed;
        ProcessedAtUtc = processedAtUtc;
        FailureReason = null;
    }

    public void MarkRejected(string reason, DateTime processedAtUtc)
    {
        Status = IntegrationWebhookDeliveryStatuses.Rejected;
        ProcessedAtUtc = processedAtUtc;
        FailureReason = Guard.AgainstNullOrWhiteSpace(reason).Trim();
    }
}

public static class IntegrationWebhookDeliveryStatuses
{
    public const string Accepted = "accepted";
    public const string Processed = "processed";
    public const string Rejected = "rejected";
}
