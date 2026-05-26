// <copyright file="IQuoteManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;

namespace NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;

public interface IQuoteManagementOutbox
{
    Task EnqueueQuoteCreatedAsync(Quote quote, CancellationToken cancellationToken);

    Task EnqueueQuoteCreatedAndPersistAsync(Quote quote, CancellationToken cancellationToken);

    Task EnqueueQuoteUpdatedAsync(Quote quote, CancellationToken cancellationToken);

    Task EnqueueQuoteDeletedAsync(Quote quote, CancellationToken cancellationToken);

    Task EnqueueQuoteRestoredAsync(Quote quote, CancellationToken cancellationToken);

    Task EnqueueQuotePurgedAsync(Guid tenantId, Guid quoteId, string? quoteNumber, Guid? ownerUserId, CancellationToken cancellationToken);
}
