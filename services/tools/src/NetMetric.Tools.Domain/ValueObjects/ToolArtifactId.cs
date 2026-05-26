// <copyright file="ToolArtifactId.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct ToolArtifactId(Guid Value)
{
    public static ToolArtifactId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}
