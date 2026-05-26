// <copyright file="ToolsRetentionOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Infrastructure.Options;

public sealed class ToolsRetentionOptions
{
    public const string SectionName = "Tools:Retention";
    public int ArtifactRetentionDays { get; init; } = 30;
    public int CleanupIntervalMinutes { get; init; } = 30;
    public int HardDeleteAfterSoftDeleteDays { get; init; } = 7;
}
