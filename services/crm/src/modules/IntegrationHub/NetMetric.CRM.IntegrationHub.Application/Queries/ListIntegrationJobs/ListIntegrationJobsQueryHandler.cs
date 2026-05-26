// <copyright file="ListIntegrationJobsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.Pagination;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationJobs;

public sealed class ListIntegrationJobsQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ListIntegrationJobsQuery, PagedResult<IntegrationJobListItemDto>>
{
    public async Task<PagedResult<IntegrationJobListItemDto>> Handle(ListIntegrationJobsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var page = PageRequest.Normalize(request.Page, request.PageSize);

        var query = dbContext.IntegrationJobs.AsNoTracking().Where(x => x.TenantId == tenantId);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(request.ProviderKey))
        {
            var providerKey = request.ProviderKey.Trim();
            query = query.Where(x => x.ProviderKey == providerKey);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ScheduledAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new IntegrationJobListItemDto(
                x.Id,
                x.ProviderKey,
                x.JobType,
                x.Direction,
                x.Status,
                x.ScheduledAtUtc,
                x.NextAttemptAtUtc,
                x.CompletedAtUtc,
                x.AttemptCount,
                x.MaxAttempts,
                x.ErrorClassification,
                x.LastErrorCode,
                x.LastErrorMessage,
                x.IsReplay))
            .ToListAsync(cancellationToken);

        return PagedResult<IntegrationJobListItemDto>.Create(items, totalCount, page);
    }
}
