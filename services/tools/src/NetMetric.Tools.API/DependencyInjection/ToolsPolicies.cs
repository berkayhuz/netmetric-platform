// <copyright file="ToolsPolicies.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.API.DependencyInjection;

public static class ToolsPolicies
{
    public const string ToolsHistoryReadOwn = "tools.history.read.own";
    public const string ToolsHistoryWriteOwn = "tools.history.write.own";
    public const string ToolsHistoryDeleteOwn = "tools.history.delete.own";
    public const string ToolsArtifactDownloadOwn = "tools.artifact.download.own";
}
