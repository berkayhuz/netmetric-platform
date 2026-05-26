// <copyright file="GetTrustedDevicesQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Devices;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Devices.Queries;

public sealed record GetTrustedDevicesQuery : IRequest<Result<TrustedDevicesResponse>>;

public sealed class GetTrustedDevicesQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IIdentityAccountClient identityAccountClient)
    : IRequestHandler<GetTrustedDevicesQuery, Result<TrustedDevicesResponse>>
{
    public async Task<Result<TrustedDevicesResponse>> Handle(GetTrustedDevicesQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var devices = await identityAccountClient.GetTrustedDevicesAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);

        return Result<TrustedDevicesResponse>.Success(new TrustedDevicesResponse(devices
            .Items
            .Select(device => new TrustedDeviceResponse(
                device.Id,
                device.DeviceName ?? "Trusted device",
                device.IpAddress,
                device.UserAgent ?? string.Empty,
                device.TrustedAt,
                device.TrustedAt.AddYears(1),
                device.IsCurrent || string.Equals(device.UserAgent, currentUser.UserAgent, StringComparison.Ordinal),
                !device.IsRevoked && device.TrustedAt.AddYears(1) > clock.UtcNow))
            .ToArray()));
    }
}
