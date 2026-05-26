// <copyright file="IntegrationJobBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public sealed class IntegrationJobBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<IntegrationJobProcessingOptions> options,
    ILogger<IntegrationJobBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configured = options.Value;
        if (!configured.Enabled)
        {
            logger.LogInformation("Integration job worker is disabled by Crm:Features:IntegrationJobProcessingEnabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IIntegrationJobProcessor>();
                var processed = await processor.ProcessDueJobsAsync(stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation("Integration job worker processed {ProcessedJobCount} jobs.", processed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Integration job worker cycle failed.");
            }

            await Task.Delay(configured.PollInterval, stoppingToken);
        }
    }
}
