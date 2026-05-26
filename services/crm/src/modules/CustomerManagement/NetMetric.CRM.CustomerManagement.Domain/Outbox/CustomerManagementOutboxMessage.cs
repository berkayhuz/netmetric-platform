// <copyright file="CustomerManagementOutboxMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities.Abstractions;

namespace NetMetric.CRM.CustomerManagement.Domain.Outbox;

public sealed class CustomerManagementOutboxMessage : ITenantEntity
{
    private CustomerManagementOutboxMessage()
    {
        EventName = string.Empty;
        RoutingKey = string.Empty;
        PayloadJson = string.Empty;
        Version = [];
    }

    private CustomerManagementOutboxMessage(
        Guid tenantId,
        string eventName,
        int eventVersion,
        string routingKey,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        string? idempotencyKey) : this()
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        EventName = eventName.Trim();
        EventVersion = eventVersion;
        RoutingKey = routingKey.Trim();
        PayloadJson = payloadJson;
        OccurredAtUtc = occurredAtUtc;
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? null : correlationId.Trim();
        IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim();
    }

    public Guid Id { get; private set; }
    public Guid TenantId { get; set; }
    public string EventName { get; private set; }
    public int EventVersion { get; private set; }
    public string RoutingKey { get; private set; }
    public string PayloadJson { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; }
    public DateTimeOffset? ProcessedAtUtc { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }
    public string? LockedBy { get; private set; }
    public DateTimeOffset? NextAttemptAtUtc { get; private set; }
    public DateTimeOffset? DeadLetteredAtUtc { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? IdempotencyKey { get; private set; }
    public byte[] Version { get; private set; }

    public static CustomerManagementOutboxMessage Create(
        Guid tenantId,
        string eventName,
        int eventVersion,
        string routingKey,
        string payloadJson,
        DateTimeOffset occurredAtUtc,
        string? correlationId,
        string? idempotencyKey)
        => new(tenantId, eventName, eventVersion, routingKey, payloadJson, occurredAtUtc, correlationId, idempotencyKey);

    public void BeginProcessing(DateTimeOffset lockedUntilUtc, string workerId)
    {
        LockedUntilUtc = lockedUntilUtc;
        LockedBy = workerId.Length > 128 ? workerId[..128] : workerId;
    }

    public void MarkProcessed(DateTimeOffset utcNow)
    {
        ProcessedAtUtc = utcNow;
        LastError = null;
        NextAttemptAtUtc = null;
        LockedUntilUtc = null;
        LockedBy = null;
    }

    public void MarkFailed(string error, DateTimeOffset nextAttemptAtUtc)
    {
        AttemptCount++;
        LastError = error.Length > 1024 ? error[..1024] : error;
        NextAttemptAtUtc = nextAttemptAtUtc;
        LockedUntilUtc = null;
        LockedBy = null;
    }

    public void MarkDeadLettered(string error, DateTimeOffset utcNow)
    {
        AttemptCount++;
        LastError = error.Length > 1024 ? error[..1024] : error;
        DeadLetteredAtUtc = utcNow;
        NextAttemptAtUtc = null;
        LockedUntilUtc = null;
        LockedBy = null;
    }
}
