// <copyright file="UserTenantMembership.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.Auth.Domain.Entities;

public sealed class UserTenantMembership : AuditableEntity
{
    public Guid UserId { get; set; }
    public string Roles { get; set; } = "tenant-user";
    public string Permissions { get; set; } = "session:self,profile:self";
    public DateTime? LastRoleChangeAt { get; set; }
    public Guid? LastRoleChangedByUserId { get; set; }
    public User? User { get; set; }
    public IReadOnlyCollection<string> GetRoles() =>
        Roles.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    public IReadOnlyCollection<string> GetPermissions() =>
        Permissions.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
