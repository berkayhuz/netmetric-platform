// <copyright file="ToolsAuthorizationOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.API.Options;

public sealed class ToolsAuthorizationOptions
{
    public const string SectionName = "Tools:Authorization";
    public bool RequireAuthenticatedUserForHistory { get; init; } = true;
}
