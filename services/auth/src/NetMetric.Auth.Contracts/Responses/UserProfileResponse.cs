// <copyright file="UserProfileResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record UserProfileResponse(
    Guid UserId,
    Guid TenantId,
    string UserName,
    string Email,
    bool EmailConfirmed,
    string? FirstName,
    string? LastName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    DateTime? PasswordChangedAt);
