// <copyright file="MfaCommands.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Security;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Security.Mfa;

public sealed record GetMfaStatusQuery : IRequest<Result<MfaStatusResponse>>;

public sealed class GetMfaStatusQueryHandler(ICurrentUserAccessor currentUserAccessor, IIdentityAccountClient identityAccountClient)
    : IRequestHandler<GetMfaStatusQuery, Result<MfaStatusResponse>>
{
    public async Task<Result<MfaStatusResponse>> Handle(GetMfaStatusQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var status = await identityAccountClient.GetMfaStatusAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        return Result<MfaStatusResponse>.Success(new MfaStatusResponse(status.IsEnabled, status.HasAuthenticator, status.RecoveryCodesRemaining));
    }
}

public sealed record StartMfaSetupCommand : IRequest<Result<MfaSetupResponse>>;

public sealed class StartMfaSetupCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter)
    : IRequestHandler<StartMfaSetupCommand, Result<MfaSetupResponse>>
{
    public async Task<Result<MfaSetupResponse>> Handle(StartMfaSetupCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = RequireMfaReauth(reauthenticationService, currentUser);
        if (reauth.IsFailure)
        {
            return Result<MfaSetupResponse>.Failure(reauth.Error);
        }

        var setup = await identityAccountClient.StartMfaSetupAsync(currentUser.TenantId, currentUser.UserId, cancellationToken);
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.MfaSetupStarted, "Information", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        return Result<MfaSetupResponse>.Success(new MfaSetupResponse(setup.SharedKey, setup.AuthenticatorUri));
    }

    private static Result RequireMfaReauth(IReauthenticationService service, CurrentUser user)
        => service.EnsureSatisfied(user, new ReauthenticationRequirement(ReauthenticationOperations.ManageMfa, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));
}

public sealed record ConfirmMfaCommand(string VerificationCode) : IRequest<Result<ConfirmMfaResponse>>;

public sealed class ConfirmMfaCommandValidator : AbstractValidator<ConfirmMfaCommand>
{
    public ConfirmMfaCommandValidator() => RuleFor(x => x.VerificationCode).NotEmpty().MaximumLength(16);
}

public sealed class ConfirmMfaCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter,
    ISecurityNotificationPublisher notificationPublisher)
    : IRequestHandler<ConfirmMfaCommand, Result<ConfirmMfaResponse>>
{
    public async Task<Result<ConfirmMfaResponse>> Handle(ConfirmMfaCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(currentUser, new ReauthenticationRequirement(ReauthenticationOperations.ManageMfa, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));
        if (reauth.IsFailure)
        {
            return Result<ConfirmMfaResponse>.Failure(reauth.Error);
        }

        var result = await identityAccountClient.ConfirmMfaAsync(currentUser.TenantId, currentUser.UserId, command.VerificationCode, cancellationToken);
        if (!result.Succeeded)
        {
            return Result<ConfirmMfaResponse>.Failure(Error.Validation("MFA verification code is invalid."));
        }

        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.MfaEnabled, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.MfaEnabled, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        await notificationPublisher.PublishAsync(new SecurityNotificationRequest(currentUser.TenantId, currentUser.UserId, "mfa_enabled", currentUser.IpAddress, currentUser.UserAgent, clock.UtcNow), cancellationToken);

        return Result<ConfirmMfaResponse>.Success(new ConfirmMfaResponse(true, result.RecoveryCodes));
    }
}

public sealed record DisableMfaCommand(string VerificationCode) : IRequest<Result>;

public sealed class DisableMfaCommandValidator : AbstractValidator<DisableMfaCommand>
{
    public DisableMfaCommandValidator() => RuleFor(x => x.VerificationCode).NotEmpty().MaximumLength(16);
}

public sealed class DisableMfaCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter,
    ISecurityNotificationPublisher notificationPublisher)
    : IRequestHandler<DisableMfaCommand, Result>
{
    public async Task<Result> Handle(DisableMfaCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(currentUser, new ReauthenticationRequirement(ReauthenticationOperations.ManageMfa, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));
        if (reauth.IsFailure)
        {
            return reauth;
        }

        await identityAccountClient.DisableMfaAsync(currentUser.TenantId, currentUser.UserId, command.VerificationCode, cancellationToken);
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.MfaDisabled, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.MfaDisabled, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, null), cancellationToken);
        await notificationPublisher.PublishAsync(new SecurityNotificationRequest(currentUser.TenantId, currentUser.UserId, "mfa_disabled", currentUser.IpAddress, currentUser.UserAgent, clock.UtcNow), cancellationToken);

        return Result.Success();
    }
}
