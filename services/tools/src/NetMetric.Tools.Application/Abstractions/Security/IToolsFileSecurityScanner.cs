// <copyright file="IToolsFileSecurityScanner.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Application.Abstractions.Security;

public sealed record ToolsFileScanResult(bool IsSafe, string? Reason = null);

public interface IToolsFileSecurityScanner
{
    Task<ToolsFileScanResult> ScanAsync(string fileName, string mimeType, Stream content, CancellationToken cancellationToken);
}
