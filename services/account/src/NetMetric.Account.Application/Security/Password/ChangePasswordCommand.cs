// <copyright file="ChangePasswordCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Clock;

namespace NetMetric.Account.Application.Security.Password;

public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmNewPassword) : IRequest<Result>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(256);
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(12).MaximumLength(128);
        RuleFor(x => x.ConfirmNewPassword).Equal(x => x.NewPassword);
    }
}

public sealed class ChangePasswordCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter,
    ISecurityNotificationPublisher notificationPublisher)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.ChangePassword, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return reauth;
        }

        var identityResult = await identityAccountClient.ChangePasswordAsync(
            currentUser.TenantId,
            currentUser.UserId,
            new ChangePasswordIdentityRequest(command.CurrentPassword, command.NewPassword, RevokeOtherSessions: true),
            cancellationToken);

        if (!identityResult.Succeeded)
        {
            var message = string.Join("; ", identityResult.Failures.Select(failure => $"{failure.Code}: {failure.Message}"));
            return Result.Failure(Error.Validation(message));
        }

        var metadata = new Dictionary<string, string> { ["revokeOtherSessions"] = bool.TrueString };
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.PasswordChanged, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.PasswordChanged, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await notificationPublisher.PublishAsync(new SecurityNotificationRequest(currentUser.TenantId, currentUser.UserId, "password_changed", currentUser.IpAddress, currentUser.UserAgent, clock.UtcNow), cancellationToken);

        return Result.Success();
    }
}
