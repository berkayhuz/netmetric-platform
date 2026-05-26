// <copyright file="IPipelineManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;

public interface IPipelineManagementOutboxPublisher
{
    Task PublishAsync(PipelineManagementOutboxMessage message, CancellationToken cancellationToken);
}
