// <copyright file="GetIntegrationOverviewQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetIntegrationOverview;

public sealed class GetIntegrationOverviewQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<GetIntegrationOverviewQuery, IntegrationOverviewDto>
{
    public async Task<IntegrationOverviewDto> Handle(GetIntegrationOverviewQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);

        var connections = await dbContext.IntegrationConnections
            .Where(x => x.TenantId == tenantId)
            .Select(x => new IntegrationConnectionDto(x.Id, x.ProviderKey, x.DisplayName, x.Category, x.IsEnabled, x.HealthStatus, x.LastHealthCheckAtUtc))
            .ToListAsync(cancellationToken);

        var jobs = await dbContext.IntegrationJobs
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.ScheduledAtUtc)
            .Select(x => new IntegrationJobDto(x.Id, x.ProviderKey, x.JobType, x.Direction, x.Status, x.ScheduledAtUtc, x.CompletedAtUtc, x.AttemptCount))
            .ToListAsync(cancellationToken);

        var logs = await dbContext.IntegrationLogEntries
            .Where(x => x.TenantId == tenantId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new IntegrationLogDto(x.Id, x.ProviderKey, x.Direction, x.Status, x.Message, x.RetryCount))
            .ToListAsync(cancellationToken);

        var webhooks = await dbContext.WebhookSubscriptions
            .Where(x => x.TenantId == tenantId)
            .Select(x => new WebhookDto(x.Id, x.EventKey, x.TargetUrl, x.IsEnabled))
            .ToListAsync(cancellationToken);

        return new IntegrationOverviewDto(connections, jobs, logs, webhooks);
    }
}
