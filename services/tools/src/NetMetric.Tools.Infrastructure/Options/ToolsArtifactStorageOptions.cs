// <copyright file="ToolsArtifactStorageOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Infrastructure.Options;

public sealed class ToolsArtifactStorageOptions
{
    public const string SectionName = "Tools:ArtifactStorage";
    public string Provider { get; init; } = "Local";
    public string RootPath { get; init; } = ".local/tools-artifacts";
    public bool AllowUnsafeLocalInProduction { get; init; }
}
