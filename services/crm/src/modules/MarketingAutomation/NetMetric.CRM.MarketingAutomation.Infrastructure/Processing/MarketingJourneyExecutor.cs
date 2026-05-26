// <copyright file="MarketingJourneyExecutor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;

public sealed class MarketingJourneyExecutor(MarketingAutomationDbContext dbContext, IOptions<MarketingAutomationOptions> options) : IMarketingJourneyExecutor
{
    private readonly MarketingAutomationOptions _options = options.Value;

    public async Task<int> ProcessDueStepsAsync(CancellationToken cancellationToken)
    {
        if (!_options.EngineEnabled)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        var steps = await dbContext.JourneyStepExecutions
            .Where(x => (x.Status == JourneyStepStatuses.Queued || x.Status == JourneyStepStatuses.Retrying) && x.NextAttemptAtUtc <= now)
            .OrderBy(x => x.NextAttemptAtUtc)
            .Take(Math.Clamp(_options.BatchSize, 1, 200))
            .ToListAsync(cancellationToken);

        foreach (var step in steps)
        {
            try
            {
                step.MarkProcessing();
                step.MarkCompleted();
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                if (step.AttemptNumber < step.MaxAttempts)
                {
                    step.MarkRetry(DateTime.UtcNow.AddSeconds(_options.BaseRetryDelaySeconds * Math.Max(1, step.AttemptNumber)), MarketingUtilities.Sanitize(exception.Message));
                }
                else
                {
                    step.MarkFailed(MarketingUtilities.Sanitize(exception.Message));
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return steps.Count;
    }
}
