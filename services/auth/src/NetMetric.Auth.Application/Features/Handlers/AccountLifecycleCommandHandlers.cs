// <copyright file="AccountLifecycleCommandHandlers.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Exceptions;
using NetMetric.Auth.Application.Features.Commands;
using NetMetric.Auth.Application.Helpers;
using NetMetric.Auth.Application.Options;
using NetMetric.Auth.Application.Records;
using NetMetric.Auth.Contracts.IntegrationEvents;
using NetMetric.Auth.Contracts.Responses;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Clock;

namespace NetMetric.Auth.Application.Features.Handlers;

public sealed class ForgotPasswordCommandHandler(
    IUserRepository userRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IIntegrationEventOutbox outbox,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock,
    IOptions<AccountLifecycleOptions> lifecycleOptions)
    : IRequestHandler<ForgotPasswordCommand>
{
    public async Task Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var normalizedEmail = AuthenticationNormalization.Normalize(request.Email);
        var user = await userRepository.FindByTenantAndIdentityAsync(request.TenantId, normalizedEmail, cancellationToken);
        if (user is null || !user.IsActive || user.IsDeleted)
        {
            return;
        }

        var options = lifecycleOptions.Value;
        var utcNow = clock.UtcDateTime;
        var rawToken = tokenService.GenerateToken();
        var expiresAt = utcNow.AddMinutes(options.PasswordResetTokenMinutes);

        await tokenRepository.RevokeOutstandingAsync(
            request.TenantId,
            user.Id,
            AuthVerificationTokenPurpose.PasswordReset,
            utcNow,
            null,
            cancellationToken);

        await tokenRepository.AddAsync(
            new AuthVerificationToken
            {
                TenantId = request.TenantId,
                UserId = user.Id,
                Purpose = AuthVerificationTokenPurpose.PasswordReset,
                TokenHash = tokenService.HashToken(rawToken),
                Target = user.Email,
                ExpiresAtUtc = expiresAt,
                CreatedAt = utcNow
            },
            cancellationToken);

        await outbox.AddAsync(
            Guid.NewGuid(),
            AuthPasswordResetRequestedV1.EventName,
            AuthPasswordResetRequestedV1.EventVersion,
            AuthPasswordResetRequestedV1.RoutingKey,
            "NetMetric.Auth",
            new AuthPasswordResetRequestedV1(
                user.Id,
                user.TenantId,
                user.UserName,
                user.Email ?? request.Email,
                rawToken,
                AccountLifecycleLinkBuilder.Build(options.PublicAppBaseUrl, options.ResetPasswordPath, user.TenantId, user.Id, rawToken, user.Email),
                expiresAt),
            request.CorrelationId,
            request.TraceId,
            utcNow,
            cancellationToken);

        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.password-reset.requested", "accepted", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ResetPasswordCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IPasswordHasher<User> passwordHasher,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IUserTokenStateValidator tokenStateValidator,
    IClock clock)
    : IRequestHandler<ResetPasswordCommand>
{
    public async Task Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var utcNow = clock.UtcDateTime;
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw InvalidToken();

        var token = await tokenRepository.GetValidAsync(
                request.TenantId,
                request.UserId,
                AuthVerificationTokenPurpose.PasswordReset,
                tokenService.HashToken(request.Token),
                utcNow,
                cancellationToken)
            ?? throw InvalidToken();

        token.Consume(utcNow, request.IpAddress);
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordChangedAt = utcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.TokenVersion++;
        user.AccessFailedCount = 0;
        user.IsLocked = false;
        user.LockoutEndAt = null;
        user.ForcePasswordChange = false;
        user.UpdatedAt = utcNow;

        await userSessionRepository.RevokeAllAsync(request.TenantId, user.Id, utcNow, "password_reset", excludedSessionId: null, cancellationToken);
        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.password-reset.completed", "success", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        tokenStateValidator.Evict(request.TenantId, user.Id);
    }

    private static AuthApplicationException InvalidToken() =>
        new("Invalid token", "The supplied password reset token is invalid or expired.", (int)HttpStatusCode.BadRequest, errorCode: "invalid_password_reset_token");
}

public sealed class ConfirmEmailCommandHandler(
    IUserRepository userRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<ConfirmEmailCommand>
{
    public async Task Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var utcNow = clock.UtcDateTime;
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw InvalidToken();

        var token = await tokenRepository.GetValidAsync(
                request.TenantId,
                request.UserId,
                AuthVerificationTokenPurpose.EmailConfirmation,
                tokenService.HashToken(request.Token),
                utcNow,
                cancellationToken)
            ?? throw InvalidToken();

        token.Consume(utcNow, request.IpAddress);
        user.EmailConfirmed = true;
        user.EmailConfirmedAt = utcNow;
        user.UpdatedAt = utcNow;

        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.email.confirmed", "success", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static AuthApplicationException InvalidToken() =>
        new("Invalid token", "The supplied email confirmation token is invalid or expired.", (int)HttpStatusCode.BadRequest, errorCode: "invalid_email_confirmation_token");
}

public sealed class ResendEmailConfirmationCommandHandler(
    IUserRepository userRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IIntegrationEventOutbox outbox,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock,
    IOptions<AccountLifecycleOptions> lifecycleOptions)
    : IRequestHandler<ResendEmailConfirmationCommand>
{
    public async Task Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByTenantAndIdentityAsync(
            request.TenantId,
            AuthenticationNormalization.Normalize(request.Email),
            cancellationToken);

        if (user is null || user.EmailConfirmed || !user.IsActive || user.IsDeleted)
        {
            return;
        }

        var options = lifecycleOptions.Value;
        var utcNow = clock.UtcDateTime;
        var rawToken = tokenService.GenerateToken();
        var expiresAt = utcNow.AddMinutes(options.EmailConfirmationTokenMinutes);

        await tokenRepository.RevokeOutstandingAsync(request.TenantId, user.Id, AuthVerificationTokenPurpose.EmailConfirmation, utcNow, null, cancellationToken);
        await tokenRepository.AddAsync(
            new AuthVerificationToken
            {
                TenantId = request.TenantId,
                UserId = user.Id,
                Purpose = AuthVerificationTokenPurpose.EmailConfirmation,
                TokenHash = tokenService.HashToken(rawToken),
                Target = user.Email,
                ExpiresAtUtc = expiresAt,
                CreatedAt = utcNow
            },
            cancellationToken);

        await outbox.AddAsync(
            Guid.NewGuid(),
            AuthEmailConfirmationRequestedV1.EventName,
            AuthEmailConfirmationRequestedV1.EventVersion,
            AuthEmailConfirmationRequestedV1.RoutingKey,
            "NetMetric.Auth",
            new AuthEmailConfirmationRequestedV1(
                user.Id,
                user.TenantId,
                user.UserName,
                user.Email ?? request.Email,
                rawToken,
                AccountLifecycleLinkBuilder.Build(options.PublicAppBaseUrl, options.ConfirmEmailPath, user.TenantId, user.Id, rawToken, user.Email),
                expiresAt),
            request.CorrelationId,
            request.TraceId,
            utcNow,
            cancellationToken);

        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.email-confirmation.resent", "accepted", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IUserSessionRepository userSessionRepository,
    IPasswordHasher<User> passwordHasher,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IUserTokenStateValidator tokenStateValidator,
    IClock clock)
    : IRequestHandler<ChangePasswordCommand>
{
    public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw NotFound();

        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            throw new AuthApplicationException("Invalid password", "Current password is invalid.", (int)HttpStatusCode.BadRequest, errorCode: "invalid_current_password");
        }

        var utcNow = clock.UtcDateTime;
        user.PasswordHash = passwordHasher.HashPassword(user, request.NewPassword);
        user.PasswordChangedAt = utcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.TokenVersion++;
        user.ForcePasswordChange = false;
        user.UpdatedAt = utcNow;

        if (request.RevokeOtherSessions)
        {
            await userSessionRepository.RevokeAllAsync(request.TenantId, user.Id, utcNow, "password_changed", request.ExcludedSessionId, cancellationToken);
        }
        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.password.changed", "success", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        tokenStateValidator.Evict(request.TenantId, user.Id);
    }

    private static AuthApplicationException NotFound() =>
        new("User not found", "User could not be found.", (int)HttpStatusCode.NotFound, errorCode: "user_not_found");
}

public sealed class ChangeEmailCommandHandler(
    IUserRepository userRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IIntegrationEventOutbox outbox,
    IPasswordHasher<User> passwordHasher,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IClock clock,
    IOptions<AccountLifecycleOptions> lifecycleOptions)
    : IRequestHandler<ChangeEmailCommand>
{
    public async Task Handle(ChangeEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw new AuthApplicationException("User not found", "User could not be found.", (int)HttpStatusCode.NotFound, errorCode: "user_not_found");

        if (!string.IsNullOrWhiteSpace(request.CurrentPassword) &&
            passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            throw new AuthApplicationException("Invalid password", "Current password is invalid.", (int)HttpStatusCode.BadRequest, errorCode: "invalid_current_password");
        }

        var normalizedNewEmail = AuthenticationNormalization.Normalize(request.NewEmail);
        if (await userRepository.ExistsByEmailAsync(request.TenantId, normalizedNewEmail, cancellationToken))
        {
            throw new AuthApplicationException("Email unavailable", "The supplied email cannot be used.", (int)HttpStatusCode.Conflict, errorCode: "email_unavailable");
        }

        var options = lifecycleOptions.Value;
        var utcNow = clock.UtcDateTime;
        var rawToken = tokenService.GenerateToken();
        var expiresAt = utcNow.AddMinutes(options.EmailChangeTokenMinutes);

        await tokenRepository.RevokeOutstandingAsync(request.TenantId, user.Id, AuthVerificationTokenPurpose.EmailChange, utcNow, null, cancellationToken);
        await tokenRepository.AddAsync(
            new AuthVerificationToken
            {
                TenantId = request.TenantId,
                UserId = user.Id,
                Purpose = AuthVerificationTokenPurpose.EmailChange,
                TokenHash = tokenService.HashToken(rawToken),
                Target = normalizedNewEmail,
                ExpiresAtUtc = expiresAt,
                CreatedAt = utcNow
            },
            cancellationToken);

        await outbox.AddAsync(
            Guid.NewGuid(),
            AuthEmailChangeRequestedV1.EventName,
            AuthEmailChangeRequestedV1.EventVersion,
            AuthEmailChangeRequestedV1.RoutingKey,
            "NetMetric.Auth",
            new AuthEmailChangeRequestedV1(
                user.Id,
                user.TenantId,
                user.UserName,
                request.CurrentEmail,
                request.NewEmail.Trim(),
                rawToken,
                AccountLifecycleLinkBuilder.Build(options.PublicAppBaseUrl, options.ConfirmEmailChangePath, user.TenantId, user.Id, rawToken, request.NewEmail),
                expiresAt),
            request.CorrelationId,
            request.TraceId,
            utcNow,
            cancellationToken);

        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.email-change.requested", "accepted", user.Id, null, request.NewEmail, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class ConfirmEmailChangeCommandHandler(
    IUserRepository userRepository,
    IAuthVerificationTokenRepository tokenRepository,
    IAuthVerificationTokenService tokenService,
    IAuthAuditTrail auditTrail,
    IAuthUnitOfWork unitOfWork,
    IUserTokenStateValidator tokenStateValidator,
    IClock clock)
    : IRequestHandler<ConfirmEmailChangeCommand>
{
    public async Task Handle(ConfirmEmailChangeCommand request, CancellationToken cancellationToken)
    {
        var utcNow = clock.UtcDateTime;
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw InvalidToken();

        var normalizedNewEmail = AuthenticationNormalization.Normalize(request.NewEmail);
        if (await userRepository.ExistsByEmailAsync(request.TenantId, normalizedNewEmail, cancellationToken))
        {
            throw new AuthApplicationException("Email unavailable", "The supplied email cannot be used.", (int)HttpStatusCode.Conflict, errorCode: "email_unavailable");
        }

        var token = await tokenRepository.GetValidAsync(
                request.TenantId,
                request.UserId,
                AuthVerificationTokenPurpose.EmailChange,
                tokenService.HashToken(request.Token),
                utcNow,
                cancellationToken)
            ?? throw InvalidToken();

        if (!string.Equals(token.Target, normalizedNewEmail, StringComparison.Ordinal))
        {
            throw InvalidToken();
        }

        token.Consume(utcNow, request.IpAddress);
        user.Email = request.NewEmail.Trim();
        user.NormalizedEmail = normalizedNewEmail;
        user.EmailConfirmed = true;
        user.EmailConfirmedAt = utcNow;
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.TokenVersion++;
        user.UpdatedAt = utcNow;

        await auditTrail.WriteAsync(
            new AuthAuditRecord(request.TenantId, "auth.email-change.confirmed", "success", user.Id, null, user.Email, request.IpAddress, request.UserAgent, request.CorrelationId, request.TraceId),
            cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        tokenStateValidator.Evict(request.TenantId, user.Id);
    }

    private static AuthApplicationException InvalidToken() =>
        new("Invalid token", "The supplied email change token is invalid or expired.", (int)HttpStatusCode.BadRequest, errorCode: "invalid_email_change_token");
}

public sealed class GetUserProfileCommandHandler(IUserRepository userRepository)
    : IRequestHandler<GetUserProfileCommand, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(GetUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw new AuthApplicationException("User not found", "User could not be found.", (int)HttpStatusCode.NotFound, errorCode: "user_not_found");
        var membership = await userRepository.GetMembershipAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw new AuthApplicationException("Forbidden", "Tenant membership is required.", (int)HttpStatusCode.Forbidden, errorCode: "membership_forbidden");

        return Map(user, membership);
    }

    internal static UserProfileResponse Map(User user, UserTenantMembership membership) =>
        new(
            user.Id,
            membership.TenantId,
            user.UserName,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.FirstName,
            user.LastName,
            membership.GetRoles(),
            membership.GetPermissions(),
            user.CreatedAt,
            user.LastLoginAt,
            user.PasswordChangedAt);
}

public sealed class UpdateProfileCommandHandler(
    IUserRepository userRepository,
    IAuthUnitOfWork unitOfWork,
    IClock clock)
    : IRequestHandler<UpdateProfileCommand, UserProfileResponse>
{
    public async Task<UserProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw new AuthApplicationException("User not found", "User could not be found.", (int)HttpStatusCode.NotFound, errorCode: "user_not_found");
        var membership = await userRepository.GetMembershipAsync(request.TenantId, request.UserId, cancellationToken)
            ?? throw new AuthApplicationException("Forbidden", "Tenant membership is required.", (int)HttpStatusCode.Forbidden, errorCode: "membership_forbidden");

        user.FirstName = AuthenticationNormalization.CleanOrNull(request.FirstName);
        user.LastName = AuthenticationNormalization.CleanOrNull(request.LastName);
        user.UpdatedAt = clock.UtcDateTime;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return GetUserProfileCommandHandler.Map(user, membership);
    }
}

internal static class AccountLifecycleLinkBuilder
{
    public static string Build(string baseUrl, string path, Guid tenantId, Guid userId, string token, string? email)
    {
        var uriBuilder = new UriBuilder(new Uri(new Uri(baseUrl.TrimEnd('/')), path.TrimStart('/')));
        var query = $"tenantId={tenantId:D}&userId={userId:D}&token={Uri.EscapeDataString(token)}";
        if (!string.IsNullOrWhiteSpace(email))
        {
            query += $"&email={Uri.EscapeDataString(email)}";
        }

        uriBuilder.Query = query;
        return uriBuilder.Uri.ToString();
    }
}
