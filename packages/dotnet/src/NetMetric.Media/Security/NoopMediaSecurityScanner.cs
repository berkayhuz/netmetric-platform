// <copyright file="NoopMediaSecurityScanner.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;

namespace NetMetric.Media.Security;

public sealed class NoopMediaSecurityScanner : IMediaSecurityScanner
{
    public Task<MediaSecurityScanResult> ScanAsync(MediaSecurityScanRequest request, CancellationToken cancellationToken)
        => Task.FromResult(MediaSecurityScanResult.Safe);
}
