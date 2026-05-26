// <copyright file="ToolsDatabaseOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Persistence.Options;

public sealed class ToolsDatabaseOptions
{
    public const string SectionName = "Tools:Database";
    public int CommandTimeoutSeconds { get; init; } = 30;
    public int MaxRetryCount { get; init; } = 3;
}
