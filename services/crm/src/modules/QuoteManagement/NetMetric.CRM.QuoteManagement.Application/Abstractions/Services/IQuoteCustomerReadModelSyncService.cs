// <copyright file="IQuoteCustomerReadModelSyncService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;

public interface IQuoteCustomerReadModelSyncService
{
    Task<QuoteCustomerReadModelSyncResult> SyncAsync(Guid customerId, CancellationToken cancellationToken);
}

public sealed record QuoteCustomerReadModelSyncResult(Guid CustomerId);
