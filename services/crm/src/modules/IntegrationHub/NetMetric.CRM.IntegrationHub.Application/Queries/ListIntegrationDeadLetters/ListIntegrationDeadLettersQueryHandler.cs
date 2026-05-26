// <copyright file="ListIntegrationDeadLettersQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationDeadLetters;

public sealed class ListIntegrationDeadLettersQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ListIntegrationDeadLettersQuery, PagedResult<IntegrationDeadLetterDto>>
{
    public async Task<PagedResult<IntegrationDeadLetterDto>> Handle(ListIntegrationDeadLettersQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var page = PageRequest.Normalize(request.Page, request.PageSize);
        var query = dbContext.IntegrationDeadLetters.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.ProviderKey))
        {
            var providerKey = request.ProviderKey.Trim();
            query = query.Where(x => x.ProviderKey == providerKey);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.FailedAtUtc)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new IntegrationDeadLetterDto(
                x.Id,
                x.JobId,
                x.ProviderKey,
                x.JobType,
                x.Direction,
                x.AttemptCount,
                x.ErrorClassification,
                x.ErrorCode,
                x.SanitizedErrorMessage,
                x.FailedAtUtc,
                x.Status,
                x.ReplayedJobId,
                x.ReplayedAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<IntegrationDeadLetterDto>.Create(items, totalCount, page);
    }
}
