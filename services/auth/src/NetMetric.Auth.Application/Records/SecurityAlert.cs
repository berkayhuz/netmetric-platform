// <copyright file="SecurityAlert.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Records;

public sealed record SecurityAlert(
    string Category,
    string Severity,
    string Message,
    Guid TenantId,
    Guid? UserId,
    Guid? SessionId,
    string? CorrelationId,
    string? TraceId,
    string? Metadata = null);
