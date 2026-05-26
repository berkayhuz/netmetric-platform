// <copyright file="ToolRun.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Domain.Entities;

public sealed class ToolRun
{
    public Guid Id { get; private set; }
    public Guid OwnerUserId { get; private set; }
    public string ToolSlug { get; private set; }
    public string InputSummaryJson { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    private ToolRun()
    {
        ToolSlug = string.Empty;
        InputSummaryJson = "{}";
    }

    public ToolRun(OwnerUserId ownerUserId, ToolSlug toolSlug, string inputSummaryJson)
    {
        Id = ToolRunId.New().Value;
        OwnerUserId = ownerUserId.Value;
        ToolSlug = toolSlug.Value;
        InputSummaryJson = string.IsNullOrWhiteSpace(inputSummaryJson) ? "{}" : inputSummaryJson.Trim();
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public void SoftDelete() => DeletedAtUtc = DateTimeOffset.UtcNow;
}
