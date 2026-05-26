// <copyright file="IStaticSearchDocumentRegistry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.StaticDocuments;

public interface IStaticSearchDocumentRegistry
{
    Task<IReadOnlyCollection<SearchDocument>> GetDocumentsAsync(CancellationToken cancellationToken);
}

