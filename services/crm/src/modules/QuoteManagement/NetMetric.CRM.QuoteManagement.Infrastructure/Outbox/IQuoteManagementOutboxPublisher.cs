// <copyright file="IQuoteManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Outbox;

public interface IQuoteManagementOutboxPublisher
{
    Task PublishAsync(QuoteManagementOutboxMessage message, CancellationToken cancellationToken);
}
