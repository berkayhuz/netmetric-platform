// <copyright file="SessionSecurityOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class SessionSecurityOptions
{
    public const string SectionName = "SessionSecurity";

    public int MaxActiveSessions { get; set; } = 5;

    public int IdleTimeoutMinutes { get; set; } = 60 * 24;

    public int AbsoluteLifetimeDays { get; set; } = 14;
}
