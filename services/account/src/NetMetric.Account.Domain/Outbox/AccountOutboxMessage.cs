// <copyright file="AccountOutboxMessage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Outbox;

public sealed class AccountOutboxMessage
{
    private AccountOutboxMessage()
    {
        Type = string.Empty;
        PayloadJson = string.Empty;
        Version = [];
    }

    private AccountOutboxMessage(Guid id, TenantId tenantId, string type, string payloadJson, DateTimeOffset occurredAt, string? correlationId)
        : this()
    {
        Id = id;
        TenantId = tenantId;
        Type = type.Trim();
        PayloadJson = payloadJson;
        OccurredAt = occurredAt;
        CorrelationId = correlationId;
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public string Type { get; private set; }
    public string PayloadJson { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public DateTimeOffset? DeadLetteredAt { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public string? CorrelationId { get; private set; }
    public byte[] Version { get; private set; }

    public static AccountOutboxMessage Create(TenantId tenantId, string type, string payloadJson, DateTimeOffset occurredAt, string? correlationId)
        => new(Guid.NewGuid(), tenantId, type, payloadJson, occurredAt, correlationId);

    public void MarkProcessed(DateTimeOffset utcNow)
    {
        ProcessedAt = utcNow;
        DeadLetteredAt = null;
        LastError = null;
    }

    public void MarkFailed(string error, DateTimeOffset nextAttemptAt)
    {
        AttemptCount++;
        LastError = error.Length > 1024 ? error[..1024] : error;
        NextAttemptAt = nextAttemptAt;
    }

    public void MarkDeadLettered(string error, DateTimeOffset utcNow)
    {
        AttemptCount++;
        DeadLetteredAt = utcNow;
        LastError = error.Length > 1024 ? error[..1024] : error;
        NextAttemptAt = null;
    }
}
