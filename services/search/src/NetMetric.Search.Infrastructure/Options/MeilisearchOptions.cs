// <copyright file="MeilisearchOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Infrastructure.Options;

public sealed class MeilisearchOptions
{
    public const string SectionName = "Meilisearch";
    public const string HttpClientName = "search-meilisearch";

    public string Endpoint { get; init; } = "http://localhost:7700";
    public string ApiKey { get; init; } = string.Empty;
}
