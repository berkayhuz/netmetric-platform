// <copyright file="OverviewQueryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Membership;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Overview.Queries;
using NetMetric.Account.Contracts.Organizations;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Account.Domain.Sessions;
using NetMetric.Clock;

namespace NetMetric.Account.Application.UnitTests;

public sealed class OverviewQueryTests
{
    [Fact]
    public async Task GetAccountOverview_ShouldCreateProfileWhenMissing()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var profiles = new List<UserProfile>();
        var sessions = new List<UserSession>
        {
            UserSession.Create(Guid.NewGuid(), TenantId.From(current.TenantId), UserId.From(current.UserId), now, now.AddHours(1), "127.0.0.1", "ua")
        };

        var identity = new Mock<IIdentityAccountClient>();
        identity
            .Setup(x => x.GetSecuritySummaryAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountSecuritySummary(false, null));
        identity
            .Setup(x => x.ListMembersAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new TenantMemberIdentityResponse(
                    current.TenantId,
                    current.UserId,
                    "berkay",
                    "berkay@example.com",
                    "Berkay",
                    "Huz",
                    true,
                    [],
                    [],
                    now.UtcDateTime,
                    null)
            ]);

        var membership = new Mock<IMembershipReadService>();
        membership
            .Setup(x => x.GetMyOrganizationsAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<OrganizationMembershipSummaryResponse>());

        var handler = new GetAccountOverviewQueryHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == now),
            MockRepoList(profiles),
            MockRepoList(sessions),
            Mock.Of<IAccountDbContext>(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()) == Task.FromResult(1)),
            identity.Object,
            membership.Object);

        var result = await handler.Handle(new GetAccountOverviewQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Berkay Huz");
        profiles.Should().ContainSingle();
    }

    [Fact]
    public async Task GetAccountOverview_ShouldUseExistingProfileDisplayName()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var profile = UserProfile.Create(TenantId.From(current.TenantId), UserId.From(current.UserId), "Ada", "Lovelace", now);
        var profiles = new List<UserProfile> { profile };

        var identity = new Mock<IIdentityAccountClient>();
        identity
            .Setup(x => x.GetSecuritySummaryAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountSecuritySummary(true, now));
        identity
            .Setup(x => x.ListMembersAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new TenantMemberIdentityResponse(
                    current.TenantId,
                    current.UserId,
                    "adal",
                    "ada@example.com",
                    "Ada",
                    "Lovelace",
                    true,
                    [],
                    [],
                    now.UtcDateTime,
                    null)
            ]);

        var membership = new Mock<IMembershipReadService>();
        membership
            .Setup(x => x.GetMyOrganizationsAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<OrganizationMembershipSummaryResponse>());

        var handler = new GetAccountOverviewQueryHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == now),
            MockRepoList(profiles),
            MockRepoList(new List<UserSession>()),
            Mock.Of<IAccountDbContext>(),
            identity.Object,
            membership.Object);

        var result = await handler.Handle(new GetAccountOverviewQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Ada Lovelace");
    }

    [Fact]
    public async Task GetAccountOverview_ShouldRepairPlaceholderProfileNameFromIdentity()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var profile = UserProfile.Create(TenantId.From(current.TenantId), UserId.From(current.UserId), "New", "Member", now);
        var profiles = new List<UserProfile> { profile };

        var identity = new Mock<IIdentityAccountClient>();
        identity
            .Setup(x => x.GetSecuritySummaryAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccountSecuritySummary(false, null));
        identity
            .Setup(x => x.ListMembersAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new TenantMemberIdentityResponse(
                    current.TenantId,
                    current.UserId,
                    "berkay",
                    "berkay@example.com",
                    "Berkay",
                    "Huz",
                    true,
                    [],
                    [],
                    now.UtcDateTime,
                    null)
            ]);

        var membership = new Mock<IMembershipReadService>();
        membership
            .Setup(x => x.GetMyOrganizationsAsync(current.TenantId, current.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<OrganizationMembershipSummaryResponse>());

        var dbContext = new Mock<IAccountDbContext>();
        dbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new GetAccountOverviewQueryHandler(
            MockCurrentUser(current),
            Mock.Of<IClock>(c => c.UtcNow == now),
            MockRepoList(profiles),
            MockRepoList(new List<UserSession>()),
            dbContext.Object,
            identity.Object,
            membership.Object);

        var result = await handler.Handle(new GetAccountOverviewQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.DisplayName.Should().Be("Berkay Huz");
    }

    private static CurrentUser CreateCurrentUser() =>
        new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), DateTimeOffset.UtcNow, ["pwd"], "corr", "127.0.0.1", "tests");

    private static ICurrentUserAccessor MockCurrentUser(CurrentUser current)
        => Mock.Of<ICurrentUserAccessor>(x => x.GetRequired() == current);

    private static IRepository<IAccountDbContext, TEntity> MockRepoList<TEntity>(List<TEntity> list)
        where TEntity : class
    {
        var mock = new Mock<IRepository<IAccountDbContext, TEntity>>();
        mock.SetupGet(x => x.Query).Returns(new TestAsyncEnumerable<TEntity>(list));
        mock.Setup(x => x.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
            .Callback<TEntity, CancellationToken>((entity, _) => list.Add(entity))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }
}
