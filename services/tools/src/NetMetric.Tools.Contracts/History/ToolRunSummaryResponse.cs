// <copyright file="ToolRunSummaryResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.History;

public sealed record ToolRunSummaryResponse(
    Guid RunId,
    string ToolSlug,
    DateTimeOffset CreatedAtUtc,
    int ArtifactCount);
