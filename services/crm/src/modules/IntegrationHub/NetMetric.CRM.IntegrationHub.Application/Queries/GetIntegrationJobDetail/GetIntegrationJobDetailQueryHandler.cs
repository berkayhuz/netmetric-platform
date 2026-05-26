// <copyright file="GetIntegrationJobDetailQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationJobDetail;

public sealed class GetIntegrationJobDetailQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<GetIntegrationJobDetailQuery, IntegrationJobDetailDto>
{
    public async Task<IntegrationJobDetailDto> Handle(GetIntegrationJobDetailQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var job = await dbContext.IntegrationJobs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.JobId, cancellationToken)
            ?? throw new NotFoundAppException("Integration job not found.");

        var executions = await dbContext.IntegrationJobExecutionLogs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.JobId == job.Id)
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => new IntegrationJobExecutionLogDto(
                x.Id,
                x.AttemptNumber,
                x.Status,
                x.Message,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.CorrelationId,
                x.ErrorClassification,
                x.ErrorCode))
            .ToListAsync(cancellationToken);

        return new IntegrationJobDetailDto(
            job.Id,
            job.ProviderKey,
            job.JobType,
            job.Direction,
            job.Status,
            job.ScheduledAtUtc,
            job.NextAttemptAtUtc,
            job.StartedAtUtc,
            job.LastAttemptAtUtc,
            job.CompletedAtUtc,
            job.CancelledAtUtc,
            job.DeadLetteredAtUtc,
            job.AttemptCount,
            job.MaxAttempts,
            job.IdempotencyKey,
            job.ErrorClassification,
            job.LastErrorCode,
            job.LastErrorMessage,
            job.CorrelationId,
            job.ReplayOfJobId,
            executions);
    }
}
