// <copyright file="SecurityRevokeAndMfaTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Devices.Commands;
using NetMetric.Account.Application.Security.Mfa;
using NetMetric.Account.Application.Sessions.Commands;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Application.UnitTests;

public sealed class SecurityRevokeAndMfaTests
{
    [Fact]
    public async Task RevokeSession_ShouldProtectCurrentSession()
    {
        var current = CreateCurrentUser();
        var sut = new RevokeSessionCommandHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == DateTimeOffset.UtcNow),
            MockSessionRepo(),
            Mock.Of<IAccountDbContext>(),
            Mock.Of<IAccountAuditWriter>(),
            Mock.Of<IAccountOutboxWriter>());

        var result = await sut.Handle(new RevokeSessionCommand(current.SessionId!.Value), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("validation_error");
    }

    [Fact]
    public async Task RevokeSession_ShouldPublishSessionRevokedEvent()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var targetSession = UserSession.Create(Guid.NewGuid(), TenantId.From(current.TenantId), UserId.From(current.UserId), now, now.AddHours(1), null, "ua");
        var outbox = new Mock<IAccountOutboxWriter>();

        var sut = new RevokeSessionCommandHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == now),
            MockSessionRepo(targetSession),
            Mock.Of<IAccountDbContext>(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(1)),
            Mock.Of<IAccountAuditWriter>(),
            outbox.Object);

        var result = await sut.Handle(new RevokeSessionCommand(targetSession.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        outbox.Verify(x => x.EnqueueAsync(
            current.TenantId,
            AccountOutboxEventTypes.SessionRevoked,
            It.Is<AccountSessionRevokedEvent>(payload => payload.SessionId == targetSession.Id && payload.Reason == "user_revoked"),
            current.CorrelationId,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RevokeOtherSessions_ShouldRevokeOnlyNonCurrentActiveSessions()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var currentSession = UserSession.Create(current.SessionId!.Value, TenantId.From(current.TenantId), UserId.From(current.UserId), now, now.AddHours(1), null, "ua");
        var otherSession = UserSession.Create(Guid.NewGuid(), TenantId.From(current.TenantId), UserId.From(current.UserId), now, now.AddHours(1), null, "ua");

        var repo = MockSessionRepo(currentSession, otherSession);
        var sut = new RevokeOtherSessionsCommandHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == now),
            Mock.Of<IReauthenticationService>(r => r.EnsureSatisfied(current, It.IsAny<ReauthenticationRequirement>()) == NetMetric.Account.Application.Common.Result.Success()),
            repo,
            Mock.Of<IAccountDbContext>(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(1)),
            Mock.Of<IAccountAuditWriter>(),
            Mock.Of<ISecurityEventWriter>());

        var result = await sut.Handle(new RevokeOtherSessionsCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        currentSession.RevokedAt.Should().BeNull();
        otherSession.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeTrustedDevice_ShouldProtectCurrentDevice()
    {
        var current = CreateCurrentUser();
        var deviceId = Guid.NewGuid();
        var identity = new Mock<IIdentityAccountClient>();
        identity.Setup(x => x.GetTrustedDevicesAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevicesIdentityResponse([new TrustedDeviceIdentityResponse(deviceId, true, "this", null, null, DateTimeOffset.UtcNow, null, false)]));

        var sut = new RevokeTrustedDeviceCommandHandler(
            MockCurrentUser(current),
            MockReauth(current),
            identity.Object,
            Mock.Of<IAccountAuditWriter>(),
            Mock.Of<ISecurityEventWriter>());

        var result = await sut.Handle(new RevokeTrustedDeviceCommand(deviceId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("validation_error");
    }

    [Fact]
    public async Task RevokeOtherTrustedDevices_ShouldNoopWhenCurrentCannotBeIdentified()
    {
        var current = CreateCurrentUser();
        var identity = new Mock<IIdentityAccountClient>();
        identity.Setup(x => x.GetTrustedDevicesAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TrustedDevicesIdentityResponse([new TrustedDeviceIdentityResponse(Guid.NewGuid(), false, "other", null, null, DateTimeOffset.UtcNow, null, false)]));

        var sut = new RevokeOtherTrustedDevicesCommandHandler(
            MockCurrentUser(current),
            MockReauth(current),
            identity.Object,
            Mock.Of<IAccountAuditWriter>(),
            Mock.Of<ISecurityEventWriter>());

        var result = await sut.Handle(new RevokeOtherTrustedDevicesCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        identity.Verify(x => x.RevokeTrustedDeviceAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmMfa_ShouldFailOnInvalidCode()
    {
        var current = CreateCurrentUser();
        var identity = new Mock<IIdentityAccountClient>();
        identity.Setup(x => x.ConfirmMfaAsync(current.TenantId, current.UserId, "000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MfaConfirmResult(false, []));

        var sut = new ConfirmMfaCommandHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == DateTimeOffset.UtcNow),
            MockReauth(current),
            identity.Object,
            Mock.Of<IAccountAuditWriter>(),
            Mock.Of<ISecurityEventWriter>(),
            Mock.Of<ISecurityNotificationPublisher>());

        var result = await sut.Handle(new ConfirmMfaCommand("000000"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("validation_error");
    }

    private static IReauthenticationService MockReauth(CurrentUser current)
        => Mock.Of<IReauthenticationService>(r => r.EnsureSatisfied(current, It.IsAny<ReauthenticationRequirement>()) == NetMetric.Account.Application.Common.Result.Success());

    private static CurrentUser CreateCurrentUser()
        => new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, ["pwd", "mfa"], "corr", "127.0.0.1", "tests");

    private static ICurrentUserAccessor MockCurrentUser(CurrentUser current)
        => Mock.Of<ICurrentUserAccessor>(x => x.GetRequired() == current);

    private static IRepository<IAccountDbContext, UserSession> MockSessionRepo(params UserSession[] sessions)
    {
        var list = sessions.ToList();
        var mock = new Mock<IRepository<IAccountDbContext, UserSession>>();
        mock.SetupGet(x => x.Query).Returns(new TestAsyncEnumerable<UserSession>(list));
        mock.Setup(x => x.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<UserSession, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((System.Linq.Expressions.Expression<Func<UserSession, bool>> predicate, CancellationToken _) => list.AsQueryable().FirstOrDefault(predicate));
        return mock.Object;
    }
}
