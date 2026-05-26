// <copyright file="SupportInboxSynchronizationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetMetric.Clock;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Services;
using NetMetric.CRM.SupportInboxIntegration.Domain.Entities;
using NetMetric.Exceptions;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.Services;

public sealed class SupportInboxSynchronizationService(
    ISupportInboxIntegrationDbContext dbContext,
    IClock clock,
    ILogger<SupportInboxSynchronizationService> logger) : ISupportInboxSynchronizationService
{
    public async Task RunAsync(Guid connectionId, bool dryRun, CancellationToken cancellationToken = default)
    {
        var connection = await dbContext.Connections.FirstOrDefaultAsync(x => x.Id == connectionId, cancellationToken)
            ?? throw new NotFoundAppException("Support inbox connection not found.");

        var run = new SupportInboxSyncRun(connection.Id, dryRun, clock.UtcDateTime);
        await dbContext.SyncRuns.AddAsync(run, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            logger.LogInformation("Manual support inbox sync started for connection {ConnectionId}. DryRun={DryRun}", connection.Id, dryRun);

            throw new ForbiddenAppException("Support inbox sync is disabled until a production mailbox provider adapter and worker are configured.");
        }
        catch (Exception ex)
        {
            run.Fail(clock.UtcDateTime, ex.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
