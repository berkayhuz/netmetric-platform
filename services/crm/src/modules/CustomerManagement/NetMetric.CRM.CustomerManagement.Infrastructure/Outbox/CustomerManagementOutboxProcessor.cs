// <copyright file="CustomerManagementOutboxProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;

public sealed class CustomerManagementOutboxProcessor(
    CustomerManagementDbContext dbContext,
    ICustomerManagementOutboxPublisher publisher,
    IOptions<CustomerManagementOutboxProcessorOptions> options,
    CustomerManagementOutboxMetrics metrics,
    ILogger<CustomerManagementOutboxProcessor> logger) : ICustomerManagementOutboxProcessor
{
    private readonly string workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var value = options.Value;

        var backlog = await dbContext.OutboxMessages
            .IgnoreQueryFilters()
            .CountAsync(x =>
                x.ProcessedAtUtc == null &&
                x.DeadLetteredAtUtc == null &&
                x.AttemptCount < value.MaxRetryCount,
                cancellationToken);
        metrics.SetBacklog(backlog);

        var messages = await dbContext.OutboxMessages
            .IgnoreQueryFilters()
            .Where(x =>
                x.ProcessedAtUtc == null &&
                x.DeadLetteredAtUtc == null &&
                x.AttemptCount < value.MaxRetryCount &&
                (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now) &&
                (x.LockedUntilUtc == null || x.LockedUntilUtc <= now))
            .OrderBy(x => x.OccurredAtUtc)
            .Take(value.BatchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var message in messages)
        {
            if (await TryProcessMessageAsync(message, cancellationToken))
            {
                processed++;
            }
        }

        return processed;
    }

    private async Task<bool> TryProcessMessageAsync(CustomerManagementOutboxMessage message, CancellationToken cancellationToken)
    {
        var value = options.Value;
        var now = DateTimeOffset.UtcNow;

        try
        {
            message.BeginProcessing(now.AddSeconds(value.LeaseSeconds), workerId);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogDebug("CRM outbox message {MessageId} was leased by another worker.", message.Id);
            dbContext.ChangeTracker.Clear();
            return false;
        }

        try
        {
            await publisher.PublishAsync(message, cancellationToken);
            message.MarkProcessed(DateTimeOffset.UtcNow);
            await dbContext.SaveChangesAsync(cancellationToken);
            metrics.Published();

            logger.LogInformation(
                "CRM outbox message published. MessageId={MessageId} EventName={EventName} RoutingKey={RoutingKey}",
                message.Id,
                message.EventName,
                message.RoutingKey);

            return true;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await MarkFailedAsync(message, exception, cancellationToken);
            return false;
        }
    }

    private async Task MarkFailedAsync(CustomerManagementOutboxMessage message, Exception exception, CancellationToken cancellationToken)
    {
        var value = options.Value;
        var now = DateTimeOffset.UtcNow;
        var nextAttemptCount = message.AttemptCount + 1;

        if (nextAttemptCount >= value.MaxRetryCount)
        {
            message.MarkDeadLettered(exception.Message, now);
            metrics.DeadLettered();
            logger.LogError(
                exception,
                "CRM outbox message dead-lettered. MessageId={MessageId} EventName={EventName} AttemptCount={AttemptCount}",
                message.Id,
                message.EventName,
                nextAttemptCount);
        }
        else
        {
            var delay = CalculateBackoff(nextAttemptCount, value.MaxBackoffSeconds);
            message.MarkFailed(exception.Message, now.Add(delay));
            metrics.Failed();
            logger.LogWarning(
                exception,
                "CRM outbox publish failed. MessageId={MessageId} EventName={EventName} AttemptCount={AttemptCount} NextAttemptInSeconds={DelaySeconds}",
                message.Id,
                message.EventName,
                nextAttemptCount,
                delay.TotalSeconds);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static TimeSpan CalculateBackoff(int attemptCount, int maxBackoffSeconds)
    {
        var seconds = Math.Min(Math.Pow(2, Math.Max(0, attemptCount - 1)), maxBackoffSeconds);
        var jitter = Random.Shared.NextDouble() * Math.Min(seconds, 5);
        return TimeSpan.FromSeconds(seconds + jitter);
    }
}
