// <copyright file="AccountOutboxProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Domain.Outbox;
using NetMetric.Account.Infrastructure.IntegrationEvents;
using NetMetric.Clock;

namespace NetMetric.Account.Infrastructure.Outbox;

public sealed class AccountOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<AccountOutboxOptions> options,
    ILogger<AccountOutboxProcessor> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Value.EnableProcessor)
        {
            logger.LogInformation("Account outbox processor is disabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Account outbox processor batch failed.");
            }

            await Task.Delay(TimeSpan.FromSeconds(options.Value.PollingIntervalSeconds), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<IAccountDbContext, AccountOutboxMessage>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<IAccountDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IAccountIntegrationEventPublisher>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();

        var utcNow = clock.UtcNow;
        var messages = await repository.Query
            .Where(message =>
                message.ProcessedAt == null &&
                message.DeadLetteredAt == null &&
                message.AttemptCount < options.Value.MaxAttempts &&
                (message.NextAttemptAt == null || message.NextAttemptAt <= utcNow))
            .OrderBy(message => message.OccurredAt)
            .Take(options.Value.BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await publisher.PublishAsync(message.Type, message.PayloadJson, message.CorrelationId, cancellationToken);
                message.MarkProcessed(clock.UtcNow);
            }
            catch (Exception exception)
            {
                var reachedMaxAttempts = message.AttemptCount + 1 >= options.Value.MaxAttempts;
                if (reachedMaxAttempts)
                {
                    message.MarkDeadLettered(exception.Message, clock.UtcNow);
                }
                else
                {
                    var delay = TimeSpan.FromSeconds(Math.Min(300, Math.Pow(2, message.AttemptCount + 1)));
                    message.MarkFailed(exception.Message, clock.UtcNow.Add(delay));
                }

                logger.LogWarning(
                    exception,
                    reachedMaxAttempts
                        ? "Account outbox message moved to dead-letter state. MessageId={MessageId} Type={EventType} Attempt={Attempt}"
                        : "Account outbox message publish failed. MessageId={MessageId} Type={EventType} Attempt={Attempt}",
                    message.Id,
                    message.Type,
                    message.AttemptCount);
            }
        }

        if (messages.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
