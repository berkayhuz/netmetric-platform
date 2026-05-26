// <copyright file="HttpSearchAccessContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using NetMetric.Authorization.Claims;
using NetMetric.Search.Application.Security;

namespace NetMetric.Search.API.Security;

public sealed class HttpSearchAccessContextFactory : ISearchAccessContextFactory
{
    public SearchAccessContext Create(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return SearchAccessContext.Anonymous;
        }

        var tenantId = ReadTenantId(principal);
        var permissions = PermissionClaimReader.ReadPermissions(principal);

        return new SearchAccessContext(
            IsAuthenticated: true,
            TenantId: tenantId,
            Permissions: permissions);
    }

    private static Guid? ReadTenantId(ClaimsPrincipal principal)
    {
        var tenantClaim = principal.FindFirst("tenant_id")?.Value ??
                          principal.FindFirst("tenantId")?.Value ??
                          principal.FindFirst("tenant")?.Value;

        return Guid.TryParse(tenantClaim, out var tenantId) && tenantId != Guid.Empty
            ? tenantId
            : null;
    }
}
