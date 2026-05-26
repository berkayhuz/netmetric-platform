// <copyright file="MediaSecurityScanResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Media.Models;

public sealed record MediaSecurityScanResult(bool IsSafe, string? FailureReason)
{
    public static MediaSecurityScanResult Safe { get; } = new(true, null);

    public static MediaSecurityScanResult Unsafe(string reason) => new(false, reason);
}
