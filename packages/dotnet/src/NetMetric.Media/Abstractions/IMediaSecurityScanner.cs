// <copyright file="IMediaSecurityScanner.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Media.Models;

namespace NetMetric.Media.Abstractions;

public interface IMediaSecurityScanner
{
    Task<MediaSecurityScanResult> ScanAsync(MediaSecurityScanRequest request, CancellationToken cancellationToken);
}
