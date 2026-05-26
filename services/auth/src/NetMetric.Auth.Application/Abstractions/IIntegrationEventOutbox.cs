// <copyright file="IIntegrationEventOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Abstractions;

public interface IIntegrationEventOutbox
{
    Task AddAsync<TEvent>(
        Guid eventId,
        string eventName,
        int eventVersion,
        string routingKey,
        string source,
        TEvent payload,
        string? correlationId,
        string? traceId,
        DateTime occurredAtUtc,
        CancellationToken cancellationToken);
}
