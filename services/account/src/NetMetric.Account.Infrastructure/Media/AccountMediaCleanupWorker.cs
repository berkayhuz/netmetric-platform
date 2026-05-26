// <copyright file="AccountMediaCleanupWorker.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.Account.Infrastructure.Media;

public sealed class AccountMediaCleanupWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<AccountMediaCleanupOptions> options,
    ILogger<AccountMediaCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("Account media cleanup worker is disabled.");
            return;
        }

        using var timer = new PeriodicTimer(options.Value.Interval);
        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCleanupAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunCleanupAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<AccountMediaCleanupService>();
            await service.RunOnceAsync(stoppingToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Account media cleanup worker failed. It will retry on the next interval.");
        }
    }
}
