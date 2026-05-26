// <copyright file="ReplayIntegrationJobCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ReplayIntegrationJob;

public sealed class ReplayIntegrationJobCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ReplayIntegrationJobCommand, Guid>
{
    public async Task<Guid> Handle(ReplayIntegrationJobCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var sourceJob = await dbContext.IntegrationJobs
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.JobId, cancellationToken)
            ?? throw new NotFoundAppException("Integration job not found.");

        if (sourceJob.Status is not (IntegrationJobStatuses.Failed or IntegrationJobStatuses.DeadLettered or IntegrationJobStatuses.Canceled))
        {
            throw new ConflictAppException("Only failed, dead-lettered or canceled integration jobs can be replayed.");
        }

        var now = DateTime.UtcNow;
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? $"replay:{sourceJob.Id:N}:{now:yyyyMMddHHmmssffff}"
            : request.IdempotencyKey.Trim();

        var replay = new IntegrationJob(
            tenantId,
            sourceJob.ProviderKey,
            sourceJob.JobType,
            sourceJob.Direction,
            sourceJob.PayloadJson,
            now,
            idempotencyKey,
            sourceJob.MaxAttempts,
            sourceJob.Id);

        await dbContext.IntegrationJobs.AddAsync(replay, cancellationToken);

        var deadLetter = await dbContext.IntegrationDeadLetters
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.JobId == sourceJob.Id, cancellationToken);
        deadLetter?.MarkReplayed(replay.Id, now);

        await dbContext.IntegrationJobExecutionLogs.AddAsync(
            new IntegrationJobExecutionLog(
                tenantId,
                sourceJob.Id,
                sourceJob.AttemptCount,
                "replayed",
                $"Replay scheduled as job {replay.Id}.",
                now,
                now,
                $"{tenantId:N}-{sourceJob.Id:N}-replay"),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return replay.Id;
    }
}
