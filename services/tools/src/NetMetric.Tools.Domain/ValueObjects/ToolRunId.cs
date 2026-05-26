// <copyright file="ToolRunId.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Domain.ValueObjects;

public readonly record struct ToolRunId(Guid Value)
{
    public static ToolRunId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString("D");
}
