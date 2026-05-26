// <copyright file="SessionContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Sessions;

public sealed record UserSessionResponse(
    Guid Id,
    string? DeviceName,
    string? IpAddress,
    string UserAgent,
    string? ApproximateLocation,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastSeenAt,
    DateTimeOffset ExpiresAt,
    bool IsCurrent,
    bool IsActive);

public sealed record UserSessionsResponse(IReadOnlyCollection<UserSessionResponse> Items);
