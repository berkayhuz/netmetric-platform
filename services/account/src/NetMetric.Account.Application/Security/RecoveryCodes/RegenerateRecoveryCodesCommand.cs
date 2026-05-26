// <copyright file="RegenerateRecoveryCodesCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Security;

namespace NetMetric.Account.Application.Security.RecoveryCodes;

public sealed record RegenerateRecoveryCodesCommand : IRequest<Result<RecoveryCodesResponse>>;

public sealed class RegenerateRecoveryCodesCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter)
    : IRequestHandler<RegenerateRecoveryCodesCommand, Result<RecoveryCodesResponse>>
{
    public async Task<Result<RecoveryCodesResponse>> Handle(RegenerateRecoveryCodesCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.RegenerateRecoveryCodes, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return Result<RecoveryCodesResponse>.Failure(reauth.Error);
        }

        var result = await identityAccountClient.RegenerateRecoveryCodesAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.RecoveryCodesRegenerated, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.RecoveryCodesRegenerated, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);

        return Result<RecoveryCodesResponse>.Success(new RecoveryCodesResponse(result.RecoveryCodes));
    }
}
