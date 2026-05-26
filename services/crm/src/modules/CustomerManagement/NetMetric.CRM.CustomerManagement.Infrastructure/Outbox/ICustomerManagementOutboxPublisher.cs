// <copyright file="ICustomerManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerManagement.Domain.Outbox;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;

public interface ICustomerManagementOutboxPublisher
{
    Task PublishAsync(CustomerManagementOutboxMessage message, CancellationToken cancellationToken);
}
