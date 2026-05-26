// <copyright file="IOpportunityManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;

public interface IOpportunityManagementOutboxPublisher
{
    Task PublishAsync(OpportunityManagementOutboxMessage message, CancellationToken cancellationToken);
}
