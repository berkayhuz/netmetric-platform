// <copyright file="CustomerManagementTrashRetentionBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.TrashRetention;

public sealed class CustomerManagementTrashRetentionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<CustomerManagementTrashRetentionOptions> options,
    ILogger<CustomerManagementTrashRetentionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configured = options.Value;
        if (!configured.Enabled)
        {
            logger.LogInformation("CustomerManagement trash retention worker is disabled.");
            return;
        }

        var initialDelay = TimeSpan.FromSeconds(Math.Max(0, configured.InitialDelaySeconds));
        if (initialDelay > TimeSpan.Zero)
        {
            await Task.Delay(initialDelay, stoppingToken);
        }

        var interval = TimeSpan.FromSeconds(Math.Max(30, configured.IntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<ICustomerManagementTrashRetentionProcessor>();
                var result = await processor.ProcessCycleAsync(stoppingToken);

                if (result.ProcessedTenants > 0 || result.PurgedItems > 0)
                {
                    logger.LogInformation(
                        "CustomerManagement trash retention worker processed {TenantCount} tenant(s) and purged {PurgedCount} item(s).",
                        result.ProcessedTenants,
                        result.PurgedItems);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "CustomerManagement trash retention worker cycle failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
