// <copyright file="AccountAuditEntry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Audit;

public sealed class AccountAuditEntry
{
    private AccountAuditEntry()
    {
        EventType = string.Empty;
        Severity = AuditSeverity.Information;
        Version = [];
    }

    private AccountAuditEntry(
        Guid id,
        TenantId tenantId,
        UserId userId,
        string eventType,
        AuditSeverity severity,
        DateTimeOffset occurredAt,
        string? correlationId)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        EventType = eventType;
        Severity = severity;
        OccurredAt = occurredAt;
        CorrelationId = correlationId;
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId UserId { get; private set; }
    public string EventType { get; private set; }
    public AuditSeverity Severity { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public string? MetadataJson { get; private set; }
    public byte[] Version { get; private set; }

    public static AccountAuditEntry Create(
        TenantId tenantId,
        UserId userId,
        string eventType,
        AuditSeverity severity,
        DateTimeOffset occurredAt,
        string? correlationId)
        => new(Guid.NewGuid(), tenantId, userId, eventType.Trim(), severity, occurredAt, correlationId);

    public void AttachRequestMetadata(string? ipAddress, string? userAgent, string? metadataJson)
    {
        IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim();
        UserAgent = string.IsNullOrWhiteSpace(userAgent) ? null : userAgent.Trim();
        MetadataJson = string.IsNullOrWhiteSpace(metadataJson) ? null : metadataJson;
    }
}
