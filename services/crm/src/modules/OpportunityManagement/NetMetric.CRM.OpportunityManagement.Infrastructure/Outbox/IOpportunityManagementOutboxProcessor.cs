// <copyright file="IOpportunityManagementOutboxProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;

public interface IOpportunityManagementOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken);
}
