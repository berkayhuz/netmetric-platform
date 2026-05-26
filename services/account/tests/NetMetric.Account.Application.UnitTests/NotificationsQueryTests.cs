// <copyright file="NotificationsQueryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Moq;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Notifications.Queries;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Notifications;

namespace NetMetric.Account.Application.UnitTests;

public sealed class NotificationsQueryTests
{
    [Fact]
    public async Task GetMyNotifications_ShouldExcludePreferencesAuditEvents()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var tenantId = TenantId.From(current.TenantId);
        var userId = UserId.From(current.UserId);

        var items = new List<AccountAuditEntry>
        {
            AccountAuditEntry.Create(tenantId, userId, AccountAuditEventTypes.NotificationPreferencesUpdated, AuditSeverity.Information, now, "corr"),
            AccountAuditEntry.Create(tenantId, userId, AccountAuditEventTypes.PreferencesUpdated, AuditSeverity.Information, now.AddMinutes(-1), "corr"),
            AccountAuditEntry.Create(tenantId, userId, AccountAuditEventTypes.PasswordChanged, AuditSeverity.Warning, now.AddMinutes(-2), "corr")
        };

        var handler = new GetMyNotificationsQueryHandler(
            MockCurrentUser(current),
            MockRepoList(items),
            MockRepoList(new List<UserNotificationState>()));

        var result = await handler.Handle(new GetMyNotificationsQuery(null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        var notification = result.Value.Items.Single();
        notification.Category.Should().Be("Security");
        notification.Title.Should().Be("Security Password_changed");
    }

    [Fact]
    public async Task GetMyNotifications_ShouldRespectReadDeleteStateForUserFacingEvents()
    {
        var now = DateTimeOffset.UtcNow;
        var current = CreateCurrentUser();
        var tenantId = TenantId.From(current.TenantId);
        var userId = UserId.From(current.UserId);

        var visible = AccountAuditEntry.Create(tenantId, userId, AccountAuditEventTypes.SessionRevoked, AuditSeverity.Warning, now, "corr");
        var deleted = AccountAuditEntry.Create(tenantId, userId, AccountAuditEventTypes.EmailChanged, AuditSeverity.Information, now.AddMinutes(-1), "corr");

        var deletedState = UserNotificationState.Create(tenantId, userId, deleted.Id, now);
        deletedState.Delete(now);
        var readState = UserNotificationState.Create(tenantId, userId, visible.Id, now);
        readState.MarkRead(now);

        var handler = new GetMyNotificationsQueryHandler(
            MockCurrentUser(current),
            MockRepoList(new List<AccountAuditEntry> { visible, deleted }),
            MockRepoList(new List<UserNotificationState> { deletedState, readState }));

        var result = await handler.Handle(new GetMyNotificationsQuery("read"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().HaveCount(1);
        var notification = result.Value.Items.Single();
        notification.Id.Should().Be(visible.Id);
        notification.IsRead.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.ReadCount.Should().Be(1);
        result.Value.UnreadCount.Should().Be(0);
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
