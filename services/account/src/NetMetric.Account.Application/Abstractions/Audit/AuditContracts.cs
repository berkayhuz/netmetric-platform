// <copyright file="AuditContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Audit;

public sealed record AccountAuditWriteRequest(
    Guid TenantId,
    Guid UserId,
    string EventType,
    string Severity,
    string? CorrelationId,
    string? IpAddress,
    string? UserAgent,
    IReadOnlyDictionary<string, string>? Metadata);

public sealed record AccountAuditReadModel(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string EventType,
    string Severity,
    DateTimeOffset OccurredAt,
    string? CorrelationId,
    string? MetadataJson);

public interface IAccountAuditWriter
{
    Task WriteAsync(AccountAuditWriteRequest request, CancellationToken cancellationToken = default);
}
