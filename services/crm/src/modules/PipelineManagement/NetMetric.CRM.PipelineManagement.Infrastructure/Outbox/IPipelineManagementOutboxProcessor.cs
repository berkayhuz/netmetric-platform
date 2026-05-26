// <copyright file="IPipelineManagementOutboxProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;

public interface IPipelineManagementOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken);
}
