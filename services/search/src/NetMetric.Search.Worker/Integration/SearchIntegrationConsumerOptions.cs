// <copyright file="SearchIntegrationConsumerOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Search.Worker.Integration;

public sealed class SearchIntegrationConsumerOptions
{
    public const string SectionName = "Search:IntegrationConsumer";

    public bool Enabled { get; init; } = true;

    [Required]
    public string QueueName { get; init; } = "netmetric.search.indexer";

    [Range(0, 120)]
    public int ProcessingFailureRequeueDelaySeconds { get; init; } = 0;

    public IReadOnlyCollection<string> RoutingKeyPatterns { get; init; } =
    [
        "search.index.*",
        "search.delete.*",
        "search.reindex.*"
    ];
}
