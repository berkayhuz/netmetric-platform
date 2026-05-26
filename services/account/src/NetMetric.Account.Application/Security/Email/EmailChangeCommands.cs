// <copyright file="EmailChangeCommands.cs" company="NetMetric">
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

namespace NetMetric.Account.Application.Security.Email;

public sealed record RequestEmailChangeCommand(string NewEmail, string CurrentPassword) : IRequest<Result<EmailChangeRequestResponse>>;

public sealed class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator()
    {
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.CurrentPassword).NotEmpty().MaximumLength(256);
    }
}

public sealed class RequestEmailChangeCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IReauthenticationService reauthenticationService,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter,
    ISecurityNotificationPublisher notificationPublisher,
    IClock clock)
    : IRequestHandler<RequestEmailChangeCommand, Result<EmailChangeRequestResponse>>
{
    public async Task<Result<EmailChangeRequestResponse>> Handle(RequestEmailChangeCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var reauth = reauthenticationService.EnsureSatisfied(
            currentUser,
            new ReauthenticationRequirement(ReauthenticationOperations.RequestEmailChange, TimeSpan.FromMinutes(10), true, ["pwd", "mfa"]));

        if (reauth.IsFailure)
        {
            return Result<EmailChangeRequestResponse>.Failure(reauth.Error);
        }

        var identityResult = await identityAccountClient.RequestEmailChangeAsync(
            currentUser.TenantId,
            currentUser.UserId,
            new EmailChangeRequestIdentityRequest(command.NewEmail, command.CurrentPassword),
            cancellationToken);

        if (!identityResult.Succeeded)
        {
            var message = string.Join("; ", identityResult.Failures.Select(failure => $"{failure.Code}: {failure.Message}"));
            return Result<EmailChangeRequestResponse>.Failure(Error.Validation(message));
        }

        var metadata = new Dictionary<string, string> { ["newEmail"] = command.NewEmail };
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.EmailChangeRequested, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.EmailChangeRequested, "Warning", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await notificationPublisher.PublishAsync(new SecurityNotificationRequest(currentUser.TenantId, currentUser.UserId, "email_change_requested", currentUser.IpAddress, currentUser.UserAgent, clock.UtcNow), cancellationToken);

        return Result<EmailChangeRequestResponse>.Success(new EmailChangeRequestResponse(ConfirmationRequired: true));
    }
}

public sealed record ConfirmEmailChangeCommand(string Token) : IRequest<Result<EmailChangeConfirmResponse>>;

public sealed class ConfirmEmailChangeCommandValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator() => RuleFor(x => x.Token).NotEmpty().MaximumLength(2048);
}

public sealed class ConfirmEmailChangeCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IIdentityAccountClient identityAccountClient,
    IAccountAuditWriter auditWriter,
    ISecurityEventWriter securityEventWriter,
    ISecurityNotificationPublisher notificationPublisher,
    IClock clock)
    : IRequestHandler<ConfirmEmailChangeCommand, Result<EmailChangeConfirmResponse>>
{
    public async Task<Result<EmailChangeConfirmResponse>> Handle(ConfirmEmailChangeCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var identityResult = await identityAccountClient.ConfirmEmailChangeAsync(
            currentUser.TenantId,
            currentUser.UserId,
            new EmailChangeConfirmIdentityRequest(command.Token),
            cancellationToken);

        if (!identityResult.Succeeded || string.IsNullOrWhiteSpace(identityResult.NewEmail))
        {
            var message = identityResult.Failures.Count == 0
                ? "Email change confirmation token is invalid or expired."
                : string.Join("; ", identityResult.Failures.Select(failure => $"{failure.Code}: {failure.Message}"));

            return Result<EmailChangeConfirmResponse>.Failure(Error.Validation(message));
        }

        var metadata = new Dictionary<string, string> { ["newEmail"] = identityResult.NewEmail };
        await auditWriter.WriteAsync(new AccountAuditWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.EmailChanged, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await securityEventWriter.WriteAsync(new SecurityEventWriteRequest(currentUser.TenantId, currentUser.UserId, AccountAuditEventTypes.EmailChanged, "Critical", currentUser.CorrelationId, currentUser.IpAddress, currentUser.UserAgent, metadata), cancellationToken);
        await notificationPublisher.PublishAsync(new SecurityNotificationRequest(currentUser.TenantId, currentUser.UserId, "email_changed", currentUser.IpAddress, currentUser.UserAgent, clock.UtcNow), cancellationToken);

        return Result<EmailChangeConfirmResponse>.Success(new EmailChangeConfirmResponse(identityResult.NewEmail));
    }
}
