// <copyright file="SearchOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Infrastructure.Options;

public sealed class SearchOptions
{
    public const string SectionName = "Search";

    public string Provider { get; init; } = "Meilisearch";
    public string IndexName { get; init; } = "searchdocuments";
    public SearchStaticIndexingOptions StaticIndexing { get; init; } = new();
}

public sealed class SearchStaticIndexingOptions
{
    public bool Enabled { get; init; }
    public bool SeedOnStartup { get; init; }
    public int StartupSeedMaxAttempts { get; init; } = 5;
    public int StartupSeedRetryDelaySeconds { get; init; } = 2;
}
