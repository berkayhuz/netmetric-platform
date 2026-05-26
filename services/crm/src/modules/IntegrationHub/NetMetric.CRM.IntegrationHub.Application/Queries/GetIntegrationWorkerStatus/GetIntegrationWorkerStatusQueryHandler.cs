// <copyright file="GetIntegrationWorkerStatusQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationWorkerStatus;

public sealed class GetIntegrationWorkerStatusQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext,
    IIntegrationJobProcessingState processingState) : IRequestHandler<GetIntegrationWorkerStatusQuery, IntegrationWorkerStatusDto>
{
    public async Task<IntegrationWorkerStatusDto> Handle(GetIntegrationWorkerStatusQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var now = DateTime.UtcNow;
        var jobs = dbContext.IntegrationJobs.AsNoTracking().Where(x => x.TenantId == tenantId);

        var dueJobs = await jobs.CountAsync(
            x => (x.Status == IntegrationJobStatuses.Queued || x.Status == IntegrationJobStatuses.Retrying) &&
                 x.ScheduledAtUtc <= now &&
                 (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now),
            cancellationToken);

        var processingJobs = await jobs.CountAsync(x => x.Status == IntegrationJobStatuses.Processing, cancellationToken);
        var retryingJobs = await jobs.CountAsync(x => x.Status == IntegrationJobStatuses.Retrying, cancellationToken);
        var deadLetteredJobs = await jobs.CountAsync(x => x.Status == IntegrationJobStatuses.DeadLettered, cancellationToken);

        return new IntegrationWorkerStatusDto(
            processingState.IsEnabled,
            dueJobs,
            processingJobs,
            retryingJobs,
            deadLetteredJobs,
            now);
    }
}
