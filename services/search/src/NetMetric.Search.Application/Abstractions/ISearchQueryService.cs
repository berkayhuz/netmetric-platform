// <copyright file="ISearchQueryService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Application.Queries;
using NetMetric.Search.Application.Security;

namespace NetMetric.Search.Application.Abstractions;

public interface ISearchQueryService
{
    Task<SearchResponse> SearchAsync(
        SearchRequest request,
        SearchAccessContext accessContext,
        CancellationToken cancellationToken);
}
