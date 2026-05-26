// <copyright file="IntegrationEventMetadata.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Messaging.Abstractions;

public sealed record IntegrationEventMetadata(
    Guid EventId,
    string EventName,
    int EventVersion,
    string Source,
    DateTime OccurredAtUtc,
    string? CorrelationId,
    string? TraceId);
