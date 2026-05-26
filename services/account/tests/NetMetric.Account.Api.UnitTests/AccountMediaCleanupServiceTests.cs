// <copyright file="AccountMediaCleanupServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Account.Infrastructure.Media;
using NetMetric.Clock;
using NetMetric.Media.Abstractions;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace NetMetric.Account.Api.UnitTests;

public sealed class AccountMediaCleanupServiceTests
{
    [Fact]
    public async Task RunOnce_ShouldDeleteOnlyOrphanPendingAvatarAssets()
    {
        var now = DateTimeOffset.UtcNow;
        var tenantId = TenantId.From(Guid.NewGuid());
        var userId = UserId.From(Guid.NewGuid());
        var orphanAsset = CreateAvatarAsset(tenantId, userId, "orphan.png", now);
        var currentAsset = CreateAvatarAsset(tenantId, userId, "current.png", now);
        orphanAsset.MarkPendingCleanup(now.AddMinutes(-10));
        currentAsset.MarkPendingCleanup(now.AddMinutes(-10));

        var profile = UserProfile.Create(tenantId, userId, "Ada", "Lovelace", now);
        profile.AssignManagedAvatar(currentAsset.Id, currentAsset.PublicUrl, now);

        var mediaAssetService = new Mock<IMediaAssetService>();
        var dbContext = new Mock<IAccountDbContext>();
        dbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var service = new AccountMediaCleanupService(
            MockRepo([orphanAsset, currentAsset]),
            MockRepo([profile]),
            dbContext.Object,
            mediaAssetService.Object,
            Mock.Of<IClock>(c => c.UtcNow == now),
            OptionsFactory.Create(new AccountMediaCleanupOptions { GracePeriodMinutes = 5, BatchSize = 10 }),
            NullLogger<AccountMediaCleanupService>.Instance);

        var cleaned = await service.RunOnceAsync(CancellationToken.None);

        cleaned.Should().Be(1);
        orphanAsset.Status.Should().Be("deleted");
        currentAsset.Status.Should().Be("cleanup_pending");
        mediaAssetService.Verify(x => x.DeleteAsync(orphanAsset.StorageKey, It.IsAny<CancellationToken>()), Times.Once);
        mediaAssetService.Verify(x => x.DeleteAsync(currentAsset.StorageKey, It.IsAny<CancellationToken>()), Times.Never);
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunOnce_ShouldLeaveAssetPendingWhenStorageDeleteFails()
    {
        var now = DateTimeOffset.UtcNow;
        var tenantId = TenantId.From(Guid.NewGuid());
        var userId = UserId.From(Guid.NewGuid());
        var asset = CreateAvatarAsset(tenantId, userId, "retry.png", now);
        asset.MarkPendingCleanup(now.AddMinutes(-10));

        var mediaAssetService = new Mock<IMediaAssetService>();
        mediaAssetService
            .Setup(x => x.DeleteAsync(asset.StorageKey, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("storage unavailable"));

        var dbContext = new Mock<IAccountDbContext>();
        var service = new AccountMediaCleanupService(
            MockRepo([asset]),
            MockRepo<UserProfile>([]),
            dbContext.Object,
            mediaAssetService.Object,
            Mock.Of<IClock>(c => c.UtcNow == now),
            OptionsFactory.Create(new AccountMediaCleanupOptions { GracePeriodMinutes = 5, BatchSize = 10 }),
            NullLogger<AccountMediaCleanupService>.Instance);

        var cleaned = await service.RunOnceAsync(CancellationToken.None);

        cleaned.Should().Be(0);
        asset.Status.Should().Be("cleanup_pending");
        dbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static IRepository<IAccountDbContext, TEntity> MockRepo<TEntity>(List<TEntity> entities)
        where TEntity : class
    {
        var mock = new Mock<IRepository<IAccountDbContext, TEntity>>();
        mock.Setup(x => x.ListAsync(It.IsAny<Expression<Func<TEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<TEntity, bool>> predicate, CancellationToken _) => entities.AsQueryable().Where(predicate).ToList());
        mock.Setup(x => x.AnyAsync(It.IsAny<Expression<Func<TEntity, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<TEntity, bool>> predicate, CancellationToken _) => entities.AsQueryable().Any(predicate));
        return mock.Object;
    }

    private static AccountMediaAsset CreateAvatarAsset(TenantId tenantId, UserId userId, string fileName, DateTimeOffset now) =>
        AccountMediaAsset.CreateAvatar(
            tenantId,
            userId,
            fileName,
            fileName,
            "image/png",
            ".png",
            128,
            new string('a', 64),
            1,
            1,
            "LocalFile",
            $"netmetric/media/tenant/avatar/{fileName}",
            $"http://localhost:5301/uploads/netmetric/media/tenant/avatar/{fileName}",
            now);
}
