// <copyright file="PipelineManagementOutboxBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;

public sealed class PipelineManagementOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<PipelineManagementOutboxProcessorOptions> options,
    ILogger<PipelineManagementOutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("PipelineManagement outbox processor is disabled.");
            return;
        }

        logger.LogInformation("PipelineManagement outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IPipelineManagementOutboxProcessor>();
                var processed = await processor.ProcessBatchAsync(stoppingToken);
                if (processed == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "PipelineManagement outbox processor loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
            }
        }
    }
}
