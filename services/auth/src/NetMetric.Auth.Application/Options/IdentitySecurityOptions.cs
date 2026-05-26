// <copyright file="IdentitySecurityOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class IdentitySecurityOptions
{
    public const string SectionName = "IdentitySecurity";

    public int MaxFailedAccessAttempts { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public List<Guid> AllowedPublicTenantIds { get; set; } = [];
}
