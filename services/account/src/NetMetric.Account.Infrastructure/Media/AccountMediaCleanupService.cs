// <copyright file="AccountMediaCleanupService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Domain.Preferences;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Clock;
using NetMetric.Media.Abstractions;

namespace NetMetric.Account.Infrastructure.Media;

public sealed class AccountMediaCleanupService(
    IRepository<IAccountDbContext, AccountMediaAsset> mediaAssets,
    IRepository<IAccountDbContext, UserProfile> profiles,
    IRepository<IAccountDbContext, UserPreference> preferences,
    IAccountDbContext dbContext,
    IMediaAssetService mediaAssetService,
    IClock clock,
    IOptions<AccountMediaCleanupOptions> options,
    ILogger<AccountMediaCleanupService> logger)
{
    public async Task<int> RunOnceAsync(CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return 0;
        }

        var cutoff = clock.UtcNow.Subtract(options.Value.GracePeriod);
        var candidates = await mediaAssets.ListAsync(
            asset => asset.Status == "cleanup_pending" &&
                     asset.DeletedAtUtc.HasValue &&
                     asset.DeletedAtUtc.Value <= cutoff,
            cancellationToken);

        var cleaned = 0;
        foreach (var asset in candidates.Take(options.Value.BatchSize))
        {
            var stillInUse = await profiles.AnyAsync(profile => profile.AvatarMediaAssetId == asset.Id, cancellationToken) ||
                             await preferences.AnyAsync(preference => preference.FaviconMediaAssetId == asset.Id, cancellationToken);
            if (stillInUse)
            {
                continue;
            }

            try
            {
                await mediaAssetService.DeleteAsync(asset.StorageKey, cancellationToken);
                asset.MarkDeleted(clock.UtcNow);
                cleaned++;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Avatar media cleanup failed for media asset {MediaAssetId}. It will be retried later.", asset.Id);
            }
        }

        if (cleaned > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return cleaned;
    }
}
