// <copyright file="IPipelineManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.PipelineManagement.Domain.Entities;

namespace NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;

public interface IPipelineManagementOutbox
{
    Task EnqueuePipelineCreatedAsync(Pipeline pipeline, CancellationToken cancellationToken);

    Task EnqueuePipelineUpdatedAsync(Pipeline pipeline, CancellationToken cancellationToken);

    Task EnqueuePipelineDeletedAsync(Pipeline pipeline, CancellationToken cancellationToken);
}
