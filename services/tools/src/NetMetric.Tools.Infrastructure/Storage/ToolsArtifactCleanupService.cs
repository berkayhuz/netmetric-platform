// <copyright file="ToolsArtifactCleanupService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Storage;
using NetMetric.Tools.Infrastructure.Options;

namespace NetMetric.Tools.Infrastructure.Storage;

public sealed class ToolsArtifactCleanupService(
    IServiceScopeFactory scopeFactory,
    IOptions<ToolsRetentionOptions> options,
    ILogger<ToolsArtifactCleanupService> logger) : BackgroundService
{
    private readonly ToolsRetentionOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Tools cleanup run failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.CleanupIntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IToolsDbContext>();
        var storage = scope.ServiceProvider.GetRequiredService<IToolArtifactStorage>();
        var now = DateTimeOffset.UtcNow;
        var retentionCutoff = now.AddDays(-_options.ArtifactRetentionDays);
        var hardDeleteCutoff = now.AddDays(-_options.HardDeleteAfterSoftDeleteDays);

        var expired = await db.ToolArtifacts
            .Where(x => (x.ExpiresAtUtc != null && x.ExpiresAtUtc < now) || (x.DeletedAtUtc != null && x.DeletedAtUtc < hardDeleteCutoff) || x.CreatedAtUtc < retentionCutoff)
            .ToListAsync(cancellationToken);

        foreach (var artifact in expired)
        {
            try
            {
                await storage.DeleteAsync(artifact.StorageKey, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Artifact delete failed for key {StorageKey}. Will retry next run.", artifact.StorageKey);
                continue;
            }
            db.ToolArtifacts.Remove(artifact);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
