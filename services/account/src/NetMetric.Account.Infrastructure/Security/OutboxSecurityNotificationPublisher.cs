// <copyright file="OutboxSecurityNotificationPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Infrastructure.Outbox;

namespace NetMetric.Account.Infrastructure.Security;

public sealed class OutboxSecurityNotificationPublisher(IAccountOutboxWriter outboxWriter) : ISecurityNotificationPublisher
{
    public Task PublishAsync(SecurityNotificationRequest request, CancellationToken cancellationToken = default)
        => outboxWriter.EnqueueAsync(
            request.TenantId,
            OutboxEventTypes.SecurityNotificationRequested,
            request,
            correlationId: null,
            cancellationToken);
}
