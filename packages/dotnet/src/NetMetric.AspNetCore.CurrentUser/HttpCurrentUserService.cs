// <copyright file="HttpCurrentUserService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NetMetric.Authorization.Claims;
using NetMetric.CurrentUser;
using NetMetric.Tenancy;

namespace NetMetric.AspNetCore.CurrentUser;

public sealed class HttpCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService, ITenantContext, ITenantProvider
{
    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

    public Guid UserId => ReadGuid(ClaimTypes.NameIdentifier, "sub");

    public Guid TenantId => ReadGuid("tenant_id", "tenantId", "tenant");

    Guid? ITenantContext.TenantId => TenantId == Guid.Empty ? null : TenantId;

    Guid? ITenantProvider.TenantId => TenantId == Guid.Empty ? null : TenantId;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated == true && UserId != Guid.Empty;

    public string? UserName =>
        Principal?.FindFirst(ClaimTypes.Name)?.Value ??
        Principal?.FindFirst("name")?.Value ??
        Principal?.Identity?.Name;

    public string? Email =>
        Principal?.FindFirst(ClaimTypes.Email)?.Value ??
        Principal?.FindFirst("email")?.Value;

    public IReadOnlyCollection<string> Roles => PermissionClaimReader.ReadRoles(Principal);

    public IReadOnlyCollection<string> Permissions => PermissionClaimReader.ReadPermissions(Principal);

    public bool IsInRole(string role) => Principal?.IsInRole(role) == true || Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) => PermissionClaimReader.HasPermission(Principal, permission);

    private Guid ReadGuid(params string[] claimTypes)
    {
        var value = claimTypes
            .Select(type => Principal?.FindFirst(type)?.Value)
            .FirstOrDefault(candidate => !string.IsNullOrWhiteSpace(candidate));

        return Guid.TryParse(value, out var parsed) ? parsed : Guid.Empty;
    }

}
