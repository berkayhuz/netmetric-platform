// <copyright file="AnalyticsProjectionBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

public sealed class AnalyticsProjectionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<AnalyticsProjectionOptions> options,
    ILogger<AnalyticsProjectionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var projectionOptions = options.Value;
        if (!projectionOptions.Enabled)
        {
            logger.LogInformation("Analytics projection worker is disabled by configuration.");
            return;
        }

        var initialDelay = TimeSpan.FromSeconds(Math.Max(0, projectionOptions.InitialDelaySeconds));
        if (initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(initialDelay, stoppingToken);
        }

        await RunProjectionAsync(stoppingToken);

        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(Math.Max(60, projectionOptions.IntervalSeconds)));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunProjectionAsync(stoppingToken);
        }
    }

    private async Task RunProjectionAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<IAnalyticsProjectionService>();
            var result = await service.RunOnceAsync(stoppingToken);
            if (!result.Succeeded)
            {
                logger.LogWarning(
                    "Analytics projection worker completed with failure state. CorrelationId={CorrelationId} Error={Error}",
                    result.CorrelationId,
                    result.ErrorMessage);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Analytics projection worker failed before projection state could be persisted.");
        }
    }
}
