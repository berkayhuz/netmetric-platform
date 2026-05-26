// <copyright file="AuthAuditRecord.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Records;

public sealed record AuthAuditRecord(
    Guid TenantId,
    string EventType,
    string Outcome,
    Guid? UserId,
    Guid? SessionId,
    string? Identity,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? TraceId,
    string? Metadata = null);
