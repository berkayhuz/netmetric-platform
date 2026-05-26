// <copyright file="NoOpToolsFileSecurityScanner.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Application.Abstractions.Security;

namespace NetMetric.Tools.Infrastructure.Security;

public sealed class NoOpToolsFileSecurityScanner : IToolsFileSecurityScanner
{
    public Task<ToolsFileScanResult> ScanAsync(string fileName, string mimeType, Stream content, CancellationToken cancellationToken)
        => Task.FromResult(new ToolsFileScanResult(true));
}
