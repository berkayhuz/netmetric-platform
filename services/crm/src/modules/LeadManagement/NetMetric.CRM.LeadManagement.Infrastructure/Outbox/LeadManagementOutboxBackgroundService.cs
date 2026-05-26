// <copyright file="LeadManagementOutboxBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Outbox;

public sealed class LeadManagementOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<LeadManagementOutboxProcessorOptions> options,
    ILogger<LeadManagementOutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("LeadManagement outbox processor is disabled.");
            return;
        }

        logger.LogInformation("LeadManagement outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ILeadManagementOutboxProcessor>();
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
                logger.LogError(exception, "LeadManagement outbox processor loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
            }
        }
    }
}
