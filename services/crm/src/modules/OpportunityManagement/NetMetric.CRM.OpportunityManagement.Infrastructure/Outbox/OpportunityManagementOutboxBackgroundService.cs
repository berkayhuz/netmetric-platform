// <copyright file="OpportunityManagementOutboxBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;

public sealed class OpportunityManagementOutboxBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OpportunityManagementOutboxProcessorOptions> options,
    ILogger<OpportunityManagementOutboxBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.Enabled)
        {
            logger.LogInformation("OpportunityManagement outbox processor is disabled.");
            return;
        }

        logger.LogInformation("OpportunityManagement outbox processor started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IOpportunityManagementOutboxProcessor>();
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
                logger.LogError(exception, "OpportunityManagement outbox processor loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(options.Value.PollIntervalSeconds), stoppingToken);
            }
        }
    }
}


