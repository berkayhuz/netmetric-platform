// <copyright file="SearchRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Queries;

public sealed record SearchRequest(
    string? Query = null,
    IReadOnlyCollection<SearchDocumentSource>? Sources = null,
    string? Type = null,
    string? Locale = null,
    IReadOnlyCollection<string>? Tags = null,
    int? Page = null,
    int? PageSize = null,
    bool IncludeDeleted = false,
    string? Sort = null);

public static class SearchRequestDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
}
