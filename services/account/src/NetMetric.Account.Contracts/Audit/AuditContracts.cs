// <copyright file="AuditContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Audit;

public sealed record AccountAuditEntryResponse(
    Guid Id,
    Guid TenantId,
    Guid UserId,
    string EventType,
    string Severity,
    DateTimeOffset OccurredAt,
    string? CorrelationId,
    string? MetadataJson);

public sealed record AccountAuditEntriesResponse(
    IReadOnlyCollection<AccountAuditEntryResponse> Items,
    int Count);
