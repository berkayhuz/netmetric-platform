// <copyright file="ICustomerManagementOutboxProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;

public interface ICustomerManagementOutboxProcessor
{
    Task<int> ProcessBatchAsync(CancellationToken cancellationToken);
}
