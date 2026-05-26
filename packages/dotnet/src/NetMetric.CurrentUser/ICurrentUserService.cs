// <copyright file="ICurrentUserService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CurrentUser;

public interface ICurrentUserService
{
    Guid UserId { get; }
    Guid TenantId { get; }
    bool IsAuthenticated { get; }
    string? UserName { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> Permissions { get; }
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
