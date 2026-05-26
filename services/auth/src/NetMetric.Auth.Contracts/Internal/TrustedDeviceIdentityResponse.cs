// <copyright file="TrustedDeviceIdentityResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Internal;

public sealed record TrustedDeviceIdentityResponse(
    Guid Id,
    bool IsCurrent,
    string? DeviceName,
    string? IpAddress,
    string? UserAgent,
    DateTimeOffset TrustedAt,
    DateTimeOffset? LastSeenAt,
    bool IsRevoked);
