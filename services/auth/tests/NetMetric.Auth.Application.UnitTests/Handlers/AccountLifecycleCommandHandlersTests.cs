// <copyright file="AccountLifecycleCommandHandlersTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using NetMetric.Auth.Application.Abstractions;
using NetMetric.Auth.Application.Exceptions;
using NetMetric.Auth.Application.Features.Commands;
using NetMetric.Auth.Application.Features.Handlers;
using NetMetric.Auth.Application.Options;
using NetMetric.Auth.Contracts.IntegrationEvents;
using NetMetric.Auth.Domain.Entities;
using NetMetric.Auth.TestKit.Builders;
using NetMetric.Auth.TestKit.Fakes;

namespace NetMetric.Auth.Application.UnitTests.Handlers;

public sealed class AccountLifecycleCommandHandlersTests
{
    [Fact]
    public async Task ForgotPassword_Should_Be_Noop_For_Unknown_User()
    {
        var tenantId = Guid.NewGuid();
        var fixture = new LifecycleFixture(new FakeClock(new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)));
        fixture.UserRepository.Setup(repository => repository.FindByTenantAndIdentityAsync(tenantId, "UNKNOWN@EXAMPLE.COM", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var sut = fixture.CreateForgotPasswordHandler();

        await sut.Handle(new ForgotPasswordCommand(tenantId, "unknown@example.com", null, null, null, null), CancellationToken.None);

        fixture.Outbox.Verify(outbox => outbox.AddAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<AuthPasswordResetRequestedV1>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<DateTime>(),
            It.IsAny<CancellationToken>()), Times.Never);
        fixture.AuthUnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ForgotPassword_Should_Create_Reset_Token_And_Outbox_Message_For_Active_User()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User().WithTenant(tenantId).Build();
        user.EmailConfirmed = true;
        const string rawToken = "raw+token/with-slash";
        var encodedToken = Uri.EscapeDataString(rawToken);

        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.FindByTenantAndIdentityAsync(tenantId, user.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.TokenService.Setup(service => service.GenerateToken()).Returns(rawToken);
        fixture.TokenService.Setup(service => service.HashToken(rawToken)).Returns("raw-token-hash");
        var sut = fixture.CreateForgotPasswordHandler();

        await sut.Handle(new ForgotPasswordCommand(tenantId, user.Email!, "127.0.0.1", "unit-test", "corr", "trace"), CancellationToken.None);

        fixture.TokenRepository.Verify(repository => repository.AddAsync(
            It.Is<AuthVerificationToken>(token =>
                token.TenantId == tenantId &&
                token.UserId == user.Id &&
                token.Purpose == AuthVerificationTokenPurpose.PasswordReset &&
                token.TokenHash == "raw-token-hash"),
            It.IsAny<CancellationToken>()), Times.Once);
        fixture.Outbox.Verify(outbox => outbox.AddAsync(
            It.IsAny<Guid>(),
            AuthPasswordResetRequestedV1.EventName,
            1,
            AuthPasswordResetRequestedV1.RoutingKey,
            "NetMetric.Auth",
            It.Is<AuthPasswordResetRequestedV1>(evt =>
                evt.UserId == user.Id &&
                evt.TenantId == user.TenantId &&
                evt.Email == user.Email &&
                evt.Token == rawToken &&
                evt.ResetUrl.Contains($"tenantId={tenantId:D}", StringComparison.Ordinal) &&
                evt.ResetUrl.Contains($"userId={user.Id:D}", StringComparison.Ordinal) &&
                evt.ResetUrl.Contains($"token={encodedToken}", StringComparison.Ordinal)),
            "corr",
            "trace",
            now,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeEmail_Should_Create_EmailChange_Token_And_Outbox_Message_For_New_Address()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .WithPasswordHash("old-hash")
            .Build();
        const string rawToken = "raw-email-change-token";
        var encodedToken = Uri.EscapeDataString(rawToken);

        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.UserRepository.Setup(repository => repository.ExistsByEmailAsync(tenantId, "JANE.NEW@EXAMPLE.COM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        fixture.PasswordHasher.Setup(hasher => hasher.VerifyHashedPassword(user, user.PasswordHash, "Str0ng!Pass123"))
            .Returns(PasswordVerificationResult.Success);
        fixture.TokenService.Setup(service => service.GenerateToken()).Returns(rawToken);
        fixture.TokenService.Setup(service => service.HashToken(rawToken)).Returns("email-change-token-hash");
        var sut = fixture.CreateChangeEmailHandler();

        await sut.Handle(
            new ChangeEmailCommand(tenantId, userId, "jane.new@example.com", user.Email!, "Str0ng!Pass123", "127.0.0.1", "unit-test", "corr", "trace"),
            CancellationToken.None);

        fixture.TokenRepository.Verify(repository => repository.AddAsync(
            It.Is<AuthVerificationToken>(token =>
                token.TenantId == tenantId &&
                token.UserId == userId &&
                token.Purpose == AuthVerificationTokenPurpose.EmailChange &&
                token.TokenHash == "email-change-token-hash" &&
                token.Target == "JANE.NEW@EXAMPLE.COM"),
            It.IsAny<CancellationToken>()), Times.Once);
        fixture.Outbox.Verify(outbox => outbox.AddAsync(
            It.IsAny<Guid>(),
            AuthEmailChangeRequestedV1.EventName,
            AuthEmailChangeRequestedV1.EventVersion,
            AuthEmailChangeRequestedV1.RoutingKey,
            "NetMetric.Auth",
            It.Is<AuthEmailChangeRequestedV1>(evt =>
                evt.UserId == userId &&
                evt.TenantId == tenantId &&
                evt.CurrentEmail == "jane@example.com" &&
                evt.NewEmail == "jane.new@example.com" &&
                evt.Token == rawToken &&
                evt.ConfirmationUrl.Contains($"tenantId={tenantId:D}", StringComparison.Ordinal) &&
                evt.ConfirmationUrl.Contains($"userId={userId:D}", StringComparison.Ordinal) &&
                evt.ConfirmationUrl.Contains($"token={encodedToken}", StringComparison.Ordinal)),
            "corr",
            "trace",
            now,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_Should_Reject_Invalid_Token()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fixture = new LifecycleFixture(new FakeClock(new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)));
        var existingUser = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .WithPasswordHash("old-hash")
            .Build();
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);
        fixture.TokenRepository.Setup(repository => repository.GetValidAsync(
                tenantId,
                userId,
                AuthVerificationTokenPurpose.PasswordReset,
                "token-hash",
                It.IsAny<DateTime>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthVerificationToken?)null);
        fixture.TokenService.Setup(service => service.HashToken("token")).Returns("token-hash");
        var sut = fixture.CreateResetPasswordHandler();

        var action = async () => await sut.Handle(
            new ResetPasswordCommand(tenantId, userId, "token", "NewPassword123!", null, null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<AuthApplicationException>();
        exception.Which.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Which.ErrorCode.Should().Be("invalid_password_reset_token");
    }

    [Fact]
    public async Task ResetPassword_Should_Reject_Unknown_User_With_Invalid_Token_Error()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fixture = new LifecycleFixture(new FakeClock(new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc)));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var sut = fixture.CreateResetPasswordHandler();

        var action = async () => await sut.Handle(
            new ResetPasswordCommand(tenantId, userId, "token", "NewPassword123!", null, null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<AuthApplicationException>();
        exception.Which.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Which.ErrorCode.Should().Be("invalid_password_reset_token");
    }

    [Fact]
    public async Task ResetPassword_Should_Revoke_All_Sessions_And_Evict_Token_State()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .WithPasswordHash("old-hash")
            .Build();
        var token = new AuthVerificationToken
        {
            TenantId = tenantId,
            UserId = userId,
            Purpose = AuthVerificationTokenPurpose.PasswordReset,
            TokenHash = "token-hash",
            ExpiresAtUtc = now.AddMinutes(30)
        };

        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.TokenRepository.Setup(repository => repository.GetValidAsync(
                tenantId,
                userId,
                AuthVerificationTokenPurpose.PasswordReset,
                "token-hash",
                now,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        fixture.TokenService.Setup(service => service.HashToken("token")).Returns("token-hash");
        fixture.PasswordHasher.Setup(hasher => hasher.HashPassword(user, "NewPassword123!")).Returns("new-password-hash");

        var sut = fixture.CreateResetPasswordHandler();

        await sut.Handle(new ResetPasswordCommand(tenantId, userId, "token", "NewPassword123!", "127.0.0.1", "unit-test", "corr", "trace"), CancellationToken.None);

        token.IsConsumed.Should().BeTrue();
        user.PasswordHash.Should().Be("new-password-hash");
        user.TokenVersion.Should().Be(1);
        fixture.UserSessionRepository.Verify(repository => repository.RevokeAllAsync(
            tenantId,
            userId,
            now,
            "password_reset",
            null,
            It.IsAny<CancellationToken>()), Times.Once);
        fixture.UserTokenStateValidator.Verify(validator => validator.Evict(tenantId, userId), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_Should_Confirm_User_When_Token_Is_Valid()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .Build();
        user.EmailConfirmed = false;

        var token = new AuthVerificationToken
        {
            TenantId = tenantId,
            UserId = userId,
            Purpose = AuthVerificationTokenPurpose.EmailConfirmation,
            TokenHash = "confirm-token-hash",
            ExpiresAtUtc = now.AddMinutes(30)
        };

        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.TokenRepository.Setup(repository => repository.GetValidAsync(
                tenantId,
                userId,
                AuthVerificationTokenPurpose.EmailConfirmation,
                "confirm-token-hash",
                now,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        fixture.TokenService.Setup(service => service.HashToken("confirm-token"))
            .Returns("confirm-token-hash");

        var sut = fixture.CreateConfirmEmailHandler();

        await sut.Handle(new ConfirmEmailCommand(tenantId, userId, "confirm-token", "127.0.0.1", "unit-test", "corr", "trace"), CancellationToken.None);

        token.IsConsumed.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
        user.EmailConfirmedAt.Should().Be(now);
        fixture.AuditTrail.Verify(trail => trail.WriteAsync(
            It.Is<NetMetric.Auth.Application.Records.AuthAuditRecord>(record => record.EventType == "auth.email.confirmed"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmEmail_Should_Reject_Unknown_User_With_Invalid_Token_Error()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var sut = fixture.CreateConfirmEmailHandler();

        var action = async () => await sut.Handle(
            new ConfirmEmailCommand(tenantId, userId, "confirm-token", null, null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<AuthApplicationException>();
        exception.Which.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Which.ErrorCode.Should().Be("invalid_email_confirmation_token");
    }

    [Fact]
    public async Task ConfirmEmail_Should_Reject_Invalid_Or_Expired_Token()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .Build();
        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.TokenRepository.Setup(repository => repository.GetValidAsync(
                tenantId,
                userId,
                AuthVerificationTokenPurpose.EmailConfirmation,
                "confirm-token-hash",
                now,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuthVerificationToken?)null);
        fixture.TokenService.Setup(service => service.HashToken("confirm-token"))
            .Returns("confirm-token-hash");

        var sut = fixture.CreateConfirmEmailHandler();

        var action = async () => await sut.Handle(
            new ConfirmEmailCommand(tenantId, userId, "confirm-token", null, null, null, null),
            CancellationToken.None);

        var exception = await action.Should().ThrowAsync<AuthApplicationException>();
        exception.Which.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        exception.Which.ErrorCode.Should().Be("invalid_email_confirmation_token");
    }

    [Fact]
    public async Task ConfirmEmail_Should_Allow_Already_Confirmed_User_When_Token_Is_Valid()
    {
        var now = new DateTime(2026, 1, 4, 0, 0, 0, DateTimeKind.Utc);
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var user = AuthTestDataBuilder.User()
            .WithId(userId)
            .WithTenant(tenantId)
            .WithIdentity("jane", "jane@example.com")
            .Build();
        user.EmailConfirmed = true;
        user.EmailConfirmedAt = now.AddDays(-1);

        var token = new AuthVerificationToken
        {
            TenantId = tenantId,
            UserId = userId,
            Purpose = AuthVerificationTokenPurpose.EmailConfirmation,
            TokenHash = "confirm-token-hash",
            ExpiresAtUtc = now.AddMinutes(30)
        };

        var fixture = new LifecycleFixture(new FakeClock(now));
        fixture.UserRepository.Setup(repository => repository.GetByIdAsync(tenantId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        fixture.TokenRepository.Setup(repository => repository.GetValidAsync(
                tenantId,
                userId,
                AuthVerificationTokenPurpose.EmailConfirmation,
                "confirm-token-hash",
                now,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(token);
        fixture.TokenService.Setup(service => service.HashToken("confirm-token"))
            .Returns("confirm-token-hash");

        var sut = fixture.CreateConfirmEmailHandler();

        await sut.Handle(new ConfirmEmailCommand(tenantId, userId, "confirm-token", null, null, null, null), CancellationToken.None);

        token.IsConsumed.Should().BeTrue();
        user.EmailConfirmed.Should().BeTrue();
    }

    private sealed class LifecycleFixture(FakeClock clock)
    {
        public Mock<IUserRepository> UserRepository { get; } = new();
        public Mock<IAuthVerificationTokenRepository> TokenRepository { get; } = new();
        public Mock<IAuthVerificationTokenService> TokenService { get; } = new();
        public Mock<IIntegrationEventOutbox> Outbox { get; } = new();
        public Mock<IAuthAuditTrail> AuditTrail { get; } = new();
        public Mock<IAuthUnitOfWork> AuthUnitOfWork { get; } = new();
        public Mock<IUserSessionRepository> UserSessionRepository { get; } = new();
        public Mock<IPasswordHasher<User>> PasswordHasher { get; } = new();
        public Mock<IUserTokenStateValidator> UserTokenStateValidator { get; } = new();

        public ForgotPasswordCommandHandler CreateForgotPasswordHandler() =>
            new(
                UserRepository.Object,
                TokenRepository.Object,
                TokenService.Object,
                Outbox.Object,
                AuditTrail.Object,
                AuthUnitOfWork.Object,
                clock,
                Microsoft.Extensions.Options.Options.Create(new AccountLifecycleOptions
                {
                    PublicAppBaseUrl = "https://auth.example.com",
                    ResetPasswordPath = "/reset-password",
                    PasswordResetTokenMinutes = 30
                }));

        public ResetPasswordCommandHandler CreateResetPasswordHandler() =>
            new(
                UserRepository.Object,
                UserSessionRepository.Object,
                TokenRepository.Object,
                TokenService.Object,
                PasswordHasher.Object,
                AuditTrail.Object,
                AuthUnitOfWork.Object,
                UserTokenStateValidator.Object,
                clock);

        public ConfirmEmailCommandHandler CreateConfirmEmailHandler() =>
            new(
                UserRepository.Object,
                TokenRepository.Object,
                TokenService.Object,
                AuditTrail.Object,
                AuthUnitOfWork.Object,
                clock);

        public ChangeEmailCommandHandler CreateChangeEmailHandler() =>
            new(
                UserRepository.Object,
                TokenRepository.Object,
                TokenService.Object,
                Outbox.Object,
                PasswordHasher.Object,
                AuditTrail.Object,
                AuthUnitOfWork.Object,
                clock,
                Microsoft.Extensions.Options.Options.Create(new AccountLifecycleOptions
                {
                    PublicAppBaseUrl = "https://auth.example.com",
                    ConfirmEmailChangePath = "/confirm-email-change",
                    EmailChangeTokenMinutes = 30
                }));
    }
}
