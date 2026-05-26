// <copyright file="IDealManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.DealManagement.Infrastructure.Outbox;

public interface IDealManagementOutboxPublisher
{
    Task PublishAsync(DealManagementOutboxMessage message, CancellationToken cancellationToken);
}

