// <copyright file="RevokeTrustedDeviceCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;

namespace NetMetric.Account.Application.Devices.Commands;

public sealed record RevokeTrustedDeviceCommand(Guid DeviceId) : IRequest<Result>;

public sealed class RevokeTrustedDeviceCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter)
    : IRequestHandler<RevokeTrustedDeviceCommand, Result>
{
    public async Task<Result> Handle(RevokeTrustedDeviceCommand command, CancellationToken cancellationToken)
    {
        if (command.DeviceId == Guid.Empty)
        {
            return Result.Failure(Error.Validation("Device id is required."));
        }

        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.RevokeTrustedDevice, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return reauth;
        }

        var allDevices = await identityAccountClient.GetTrustedDevicesAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        var targetDevice = allDevices.Items.FirstOrDefault(x => x.Id == command.DeviceId);
        if (targetDevice is null)
        {
            return Result.Failure(Error.NotFound("Trusted device"));
        }

        if (targetDevice.IsCurrent)
        {
            return Result.Failure(Error.Validation("Current trusted device cannot be revoked from this endpoint."));
        }

        var revoked = await identityAccountClient.RevokeTrustedDeviceAsync(
            currentUser.TenantId,
            currentUser.UserId,
            command.DeviceId,
            cancellationToken);

        if (!revoked)
        {
            return Result.Failure(Error.NotFound("Trusted device"));
        }

        var metadata = new Dictionary<string, string> { ["deviceId"] = command.DeviceId.ToString("D") };
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);

        return Result.Success();
    }
}
