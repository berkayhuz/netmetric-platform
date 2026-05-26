// <copyright file="ToolJobContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.Jobs;

public enum ToolJobStatus
{
    Queued = 1,
    Running = 2,
    Succeeded = 3,
    Failed = 4,
    Cancelled = 5,
    Expired = 6
}

public sealed record CreateToolJobResponse(Guid JobId, ToolJobStatus Status, DateTimeOffset CreatedAtUtc);
public sealed record ToolJobStatusResponse(Guid JobId, ToolJobStatus Status, int ProgressPercent, Guid? RunId, string? Error, DateTimeOffset UpdatedAtUtc);
