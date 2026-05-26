// <copyright file="RevokeOtherTrustedDevicesCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;

namespace NetMetric.Account.Application.Devices.Commands;

public sealed record RevokeOtherTrustedDevicesCommand : IRequest<Result>;

public sealed class RevokeOtherTrustedDevicesCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter)
    : IRequestHandler<RevokeOtherTrustedDevicesCommand, Result>
{
    public async Task<Result> Handle(RevokeOtherTrustedDevicesCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.RevokeTrustedDevice, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return reauth;
        }

        var devices = await identityAccountClient.GetTrustedDevicesAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        if (!devices.Items.Any(x => x.IsCurrent))
        {
            var skippedMetadata = new Dictionary<string, string>
            {
                ["revokedCount"] = "0",
                ["skipReason"] = "active_device_not_identifiable"
            };
            await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, skippedMetadata), cancellationToken);
            await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, skippedMetadata), cancellationToken);
            return Result.Success();
        }

        var others = devices.Items.Where(x => !x.IsCurrent && !x.IsRevoked).ToArray();
        foreach (var device in others)
        {
            await identityAccountClient.RevokeTrustedDeviceAsync(currentUser.TenantId, currentUser.UserId, device.Id, cancellationToken);
        }

        var metadata = new Dictionary<string, string> { ["revokedCount"] = others.Length.ToString() };
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.TrustedDeviceRevoked, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);

        return Result.Success();
    }
}
