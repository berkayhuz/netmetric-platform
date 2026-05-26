// <copyright file="PermissionClaimReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Claims;

namespace NetMetric.Authorization.Claims;

public static class PermissionClaimReader
{
    private static readonly string[] PermissionClaimTypes = ["permission", "permissions", "scope", "scp"];
    private static readonly string[] RoleClaimTypes = [ClaimTypes.Role, "role", "roles"];
    private static readonly string[] TenantClaimTypes = ["tenant_id", "tenantId", "tenant"];

    public static IReadOnlyCollection<string> ReadPermissions(ClaimsPrincipal? principal) =>
        ReadValues(principal, PermissionClaimTypes);

    public static IReadOnlyCollection<string> ReadRoles(ClaimsPrincipal? principal) =>
        ReadValues(principal, RoleClaimTypes);

    public static bool HasPermission(ClaimsPrincipal? principal, string permission)
    {
        var permissions = ReadPermissions(principal);
        return HasTenant(principal) &&
               (permissions.Contains("*", StringComparer.OrdinalIgnoreCase) ||
                permissions.Contains(permission, StringComparer.OrdinalIgnoreCase));
    }

    public static bool HasTenant(ClaimsPrincipal? principal) =>
        TenantClaimTypes
            .Select(type => principal?.FindFirst(type)?.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Any(value => Guid.TryParse(value, out var tenantId) && tenantId != Guid.Empty);

    private static IReadOnlyCollection<string> ReadValues(ClaimsPrincipal? principal, IEnumerable<string> claimTypes) =>
        claimTypes
            .SelectMany(type => principal?.FindAll(type).Select(claim => claim.Value) ?? [])
            .SelectMany(SplitClaimValue)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static IEnumerable<string> SplitClaimValue(string value) =>
        value.Split([' ', ',', ';'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
}
