// <copyright file="SearchResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Queries;

public sealed record SearchResponse(
    string Query,
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyCollection<SearchResultItem> Items,
    bool PermissionPostFilteringApplied = false);

public sealed record SearchResultItem(
    string Id,
    SearchDocumentSource Source,
    string Type,
    string Title,
    string Summary,
    string Url,
    SearchDocumentVisibility Visibility,
    string Locale,
    IReadOnlyCollection<string> Tags,
    double? RankingScore = null,
    string? HighlightedTitle = null,
    string? HighlightedSummary = null,
    IReadOnlyCollection<SearchHighlight>? Highlights = null);

public sealed record SearchHighlight(string Field, string Value);
