// <copyright file="ILeadManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Infrastructure.Outbox;

public interface ILeadManagementOutboxPublisher
{
    Task PublishAsync(LeadManagementOutboxMessage message, CancellationToken cancellationToken);
}
