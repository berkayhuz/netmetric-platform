// <copyright file="AccountAuthorizationOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Api.Authorization;

public sealed class AccountAuthorizationOptions
{
    public const string SectionName = "Authorization";

    public bool RequireTenantClaim { get; init; } = true;
    public string RequiredPermissionClaimType { get; init; } = "permission";
    public string RequiredTenantClaimType { get; init; } = "tenant_id";
}
