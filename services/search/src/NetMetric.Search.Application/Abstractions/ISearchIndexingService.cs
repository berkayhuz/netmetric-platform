// <copyright file="ISearchIndexingService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Abstractions;

public interface ISearchIndexingService
{
    Task UpsertAsync(SearchDocument document, CancellationToken cancellationToken);

    Task UpsertManyAsync(IReadOnlyCollection<SearchDocument> documents, CancellationToken cancellationToken);

    Task SoftDeleteAsync(string documentId, CancellationToken cancellationToken);

    Task HardDeleteAsync(string documentId, CancellationToken cancellationToken);
}

