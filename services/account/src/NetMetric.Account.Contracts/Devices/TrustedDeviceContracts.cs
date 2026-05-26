// <copyright file="TrustedDeviceContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Devices;

public sealed record TrustedDeviceResponse(
    Guid Id,
    string Name,
    string? IpAddress,
    string UserAgent,
    DateTimeOffset TrustedAt,
    DateTimeOffset ExpiresAt,
    bool IsCurrent,
    bool IsActive);

public sealed record TrustedDevicesResponse(IReadOnlyCollection<TrustedDeviceResponse> Items);
