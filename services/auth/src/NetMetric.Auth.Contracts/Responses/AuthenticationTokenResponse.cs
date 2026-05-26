// <copyright file="AuthenticationTokenResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record AuthenticationTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt,
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Email,
    Guid SessionId,
    bool IsPersistent = false);
