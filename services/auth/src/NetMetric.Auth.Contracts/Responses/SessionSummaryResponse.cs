// <copyright file="SessionSummaryResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Responses;

public sealed record SessionSummaryResponse(
    Guid SessionId,
    bool IsCurrent,
    bool IsRevoked,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime? LastSeenAt,
    DateTime RefreshTokenExpiresAt,
    DateTime? RevokedAt,
    string? RevokedReason);
