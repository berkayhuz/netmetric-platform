// <copyright file="PermissionAuthorizationHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Authorization;

namespace NetMetric.Auth.API.Permissions;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var hasTenant = context.User.HasClaim(claim =>
            claim.Type == "tenant_id" &&
            Guid.TryParse(claim.Value, out var tenantId) &&
            tenantId != Guid.Empty);

        if (hasTenant &&
            (context.User.HasClaim("permission", "*") ||
            context.User.HasClaim("permission", requirement.Permission))
           )
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
