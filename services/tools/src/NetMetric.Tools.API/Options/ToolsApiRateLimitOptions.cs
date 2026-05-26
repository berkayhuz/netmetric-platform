// <copyright file="ToolsApiRateLimitOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.API.Options;

public sealed class ToolsApiRateLimitOptions
{
    public const string SectionName = "Tools:RateLimiting";
    public int HistoryWritePermitLimit { get; init; } = 20;
    public int HistoryWriteWindowSeconds { get; init; } = 60;
    public int PublicCatalogPermitLimit { get; init; } = 100;
    public int PublicCatalogWindowSeconds { get; init; } = 60;
}
