// <copyright file="AuthSessionStatusResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record AuthSessionStatusResponse(
    Guid TenantId,
    Guid UserId,
    Guid SessionId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    string AccountStatus,
    bool EmailConfirmed,
    DateTimeOffset? MfaVerifiedAt);
