// <copyright file="AccountPermissionRequirement.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;

namespace NetMetric.Account.Api.Authorization;

public sealed class AccountPermissionRequirement(string permission, string claimType) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
    public string ClaimType { get; } = claimType;
}

public sealed class AccountPermissionAuthorizationHandler : AuthorizationHandler<AccountPermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountPermissionRequirement requirement)
    {
        var hasTenant = context.User.HasClaim(claim =>
            (claim.Type == "tenant_id" || claim.Type == "tenantId" || claim.Type == "tenant") &&
            Guid.TryParse(claim.Value, out var tenantId) &&
            tenantId != Guid.Empty);

        if (hasTenant &&
            (context.User.HasClaim(requirement.ClaimType, "*") ||
            context.User.HasClaim(requirement.ClaimType, requirement.Permission))
           )
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
