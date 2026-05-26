// <copyright file="ISearchIntegrationEventIngestionService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.Search.Application.Abstractions;

public interface ISearchIntegrationEventIngestionService
{
    Task HandleAsync(SearchDocumentIndexRequestedV1 integrationEvent, CancellationToken cancellationToken);

    Task HandleAsync(SearchDocumentDeleteRequestedV1 integrationEvent, CancellationToken cancellationToken);

    Task HandleAsync(SearchReindexRequestedV1 integrationEvent, CancellationToken cancellationToken);
}
