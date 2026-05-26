// <copyright file="IntegrationJobProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public sealed class IntegrationJobProcessor(
    IntegrationHubDbContext dbContext,
    IIntegrationConnectorRegistry connectorRegistry,
    IOptions<IntegrationJobProcessingOptions> options,
    ILogger<IntegrationJobProcessor> logger) : IIntegrationJobProcessor
{
    private readonly IntegrationJobProcessingOptions _options = options.Value;
    private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    public async Task<int> ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var batchSize = Math.Clamp(_options.BatchSize, 1, 100);
        var dueJobs = await dbContext.IntegrationJobs
            .IgnoreQueryFilters()
            .Where(x =>
                (x.Status == IntegrationJobStatuses.Queued || x.Status == IntegrationJobStatuses.Retrying) &&
                x.ScheduledAtUtc <= now &&
                (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.NextAttemptAtUtc ?? x.ScheduledAtUtc)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var job in dueJobs)
        {
            if (!job.TryAcquire(_workerId, now, _options.LeaseDuration))
            {
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            await ExecuteAcquiredJobAsync(job.Id, cancellationToken);
            processed += 1;
        }

        return processed;
    }

    private async Task ExecuteAcquiredJobAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var startedAt = DateTime.UtcNow;
        var job = await dbContext.IntegrationJobs
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == jobId, cancellationToken);

        if (job.Status == IntegrationJobStatuses.CancelRequested)
        {
            job.MarkCanceled(DateTime.UtcNow);
            await AddExecutionLogAsync(job, "canceled", "Job was canceled before connector execution.", startedAt, DateTime.UtcNow, cancellationToken);
            IntegrationJobMetrics.RecordCanceled(job.ProviderKey);
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var completedDuplicate = await dbContext.IntegrationJobs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                x => x.Id != job.Id &&
                     x.TenantId == job.TenantId &&
                     x.ProviderKey == job.ProviderKey &&
                     x.IdempotencyKey == job.IdempotencyKey &&
                     x.Status == IntegrationJobStatuses.Completed,
                cancellationToken);
        if (completedDuplicate)
        {
            job.MarkCompleted(DateTime.UtcNow);
            await AddExecutionLogAsync(job, "idempotent-skip", "A completed job already exists for this idempotency key.", startedAt, DateTime.UtcNow, cancellationToken);
            IntegrationJobMetrics.RecordSucceeded(job.ProviderKey);
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        var connection = await dbContext.IntegrationConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == job.TenantId && x.ProviderKey == job.ProviderKey, cancellationToken);

        if (connection is null || !connection.IsEnabled)
        {
            var error = IntegrationErrorClassifier.NonRetryable(
                "configuration",
                "connector_connection_unavailable",
                connection is null ? "Integration connection was not found." : "Integration connection is disabled.");
            await FinalizeFailureAsync(job, error, startedAt, cancellationToken);
            return;
        }

        var connector = connectorRegistry.Resolve(job.ProviderKey);
        if (connector is null)
        {
            var error = IntegrationErrorClassifier.NonRetryable(
                "configuration",
                "connector_adapter_unregistered",
                "No connector adapter is registered for this provider.");
            await FinalizeFailureAsync(job, error, startedAt, cancellationToken);
            return;
        }

        try
        {
            var context = new IntegrationJobExecutionContext(
                job.TenantId,
                job.Id,
                job.ProviderKey,
                job.JobType,
                job.Direction,
                job.PayloadJson,
                connection.DeltaSyncToken,
                job.CorrelationId ?? $"{job.TenantId:N}-{job.Id:N}-{job.AttemptCount}");

            var result = await connector.ExecuteAsync(context, cancellationToken);
            if (!result.Succeeded)
            {
                var error = IntegrationErrorClassifier.NonRetryable("permanent", "connector_declined", result.Message);
                await FinalizeFailureAsync(job, error, startedAt, cancellationToken);
                return;
            }

            connection.UpdateDeltaSyncToken(result.DeltaSyncToken);
            connection.RecordHealth(IntegrationConnectorHealthStates.Healthy, "Last job execution succeeded.", DateTime.UtcNow);
            job.MarkCompleted(DateTime.UtcNow);
            await AddExecutionLogAsync(job, IntegrationJobStatuses.Completed, result.Message, startedAt, DateTime.UtcNow, cancellationToken);
            IntegrationJobMetrics.RecordSucceeded(job.ProviderKey);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(
                exception,
                "Integration job {JobId} for tenant {TenantId} failed with classification input.",
                job.Id,
                job.TenantId);
            await FinalizeFailureAsync(job, IntegrationErrorClassifier.Classify(exception), startedAt, cancellationToken);
        }
    }

    private async Task FinalizeFailureAsync(
        IntegrationJob job,
        ClassifiedIntegrationError error,
        DateTime startedAtUtc,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var canRetry = error.IsRetryable && job.AttemptCount < Math.Max(1, Math.Max(job.MaxAttempts, _options.MaxAttempts));

        if (canRetry)
        {
            var nextAttemptAt = now.Add(error.RetryAfter ?? ComputeRetryDelay(job.AttemptCount));
            job.MarkRetry(nextAttemptAt, error.Classification, error.ErrorCode, error.SanitizedMessage);
            await AddExecutionLogAsync(job, IntegrationJobStatuses.Retrying, error.SanitizedMessage, startedAtUtc, now, cancellationToken, error.Classification, error.ErrorCode);
            IntegrationJobMetrics.RecordRetried(job.ProviderKey);
        }
        else
        {
            job.MoveToDeadLetter(now, error.Classification, error.ErrorCode, error.SanitizedMessage);
            await EnsureDeadLetterAsync(job, error, now, cancellationToken);
            await AddExecutionLogAsync(job, IntegrationJobStatuses.DeadLettered, error.SanitizedMessage, startedAtUtc, now, cancellationToken, error.Classification, error.ErrorCode);
            IntegrationJobMetrics.RecordDeadLettered(job.ProviderKey);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private TimeSpan ComputeRetryDelay(int attemptCount)
    {
        var exponent = Math.Max(0, attemptCount - 1);
        var seconds = _options.BaseRetryDelay.TotalSeconds * Math.Pow(2, exponent);
        return TimeSpan.FromSeconds(Math.Min(seconds, _options.MaxRetryDelay.TotalSeconds));
    }

    private async Task EnsureDeadLetterAsync(
        IntegrationJob job,
        ClassifiedIntegrationError error,
        DateTime failedAtUtc,
        CancellationToken cancellationToken)
    {
        var exists = await dbContext.IntegrationDeadLetters
            .IgnoreQueryFilters()
            .AnyAsync(x => x.TenantId == job.TenantId && x.JobId == job.Id, cancellationToken);
        if (exists)
        {
            return;
        }

        await dbContext.IntegrationDeadLetters.AddAsync(
            new IntegrationDeadLetterEntry(job, error.Classification, error.ErrorCode, error.SanitizedMessage, failedAtUtc),
            cancellationToken);
    }

    private Task AddExecutionLogAsync(
        IntegrationJob job,
        string status,
        string message,
        DateTime startedAtUtc,
        DateTime? completedAtUtc,
        CancellationToken cancellationToken,
        string? errorClassification = null,
        string? errorCode = null)
        => dbContext.IntegrationJobExecutionLogs.AddAsync(
            new IntegrationJobExecutionLog(
                job.TenantId,
                job.Id,
                job.AttemptCount,
                status,
                IntegrationErrorClassifier.Sanitize(message),
                startedAtUtc,
                completedAtUtc,
                job.CorrelationId ?? $"{job.TenantId:N}-{job.Id:N}-{job.AttemptCount}",
                errorClassification,
                errorCode),
            cancellationToken).AsTask();
}
