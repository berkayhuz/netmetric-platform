// <copyright file="RoleManagementResponses.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record TenantMemberResponse(
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed record RoleCatalogResponse(
    string Name,
    int Rank,
    bool IsProtected,
    IReadOnlyCollection<string> Permissions);
