// <copyright file="IMeilisearchIndexInitializer.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Infrastructure.Meilisearch.Indexing;

internal interface IMeilisearchIndexInitializer
{
    Task EnsureInitializedAsync(CancellationToken cancellationToken);
}
