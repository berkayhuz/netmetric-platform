// <copyright file="GetMyToolRunsQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Contracts.History;

namespace NetMetric.Tools.Application.History.Queries;

public sealed record GetMyToolRunsQuery(ToolHistoryQuery Query) : IRequest<ToolHistoryPageResponse>;

public sealed class GetMyToolRunsQueryHandler(
    IToolsDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor) : IRequestHandler<GetMyToolRunsQuery, ToolHistoryPageResponse>
{
    public async Task<ToolHistoryPageResponse> Handle(GetMyToolRunsQuery request, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.GetRequired();
        var page = request.Query.Page <= 0 ? 1 : request.Query.Page;
        var pageSize = request.Query.PageSize <= 0 ? 20 : Math.Min(request.Query.PageSize, 100);

        var query = dbContext.ToolRuns.AsNoTracking().Where(x => x.OwnerUserId == user.UserId && x.DeletedAtUtc == null);

        if (!string.IsNullOrWhiteSpace(request.Query.ToolSlug))
        {
            var slug = request.Query.ToolSlug.Trim().ToLowerInvariant();
            query = query.Where(x => x.ToolSlug == slug);
        }

        var total = await query.CountAsync(cancellationToken);
        var runs = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var runIds = runs.Select(static x => x.Id).ToArray();
        var artifactCounts = await dbContext.ToolArtifacts
            .AsNoTracking()
            .Where(x => runIds.Contains(x.ToolRunId) && x.DeletedAtUtc == null)
            .GroupBy(x => x.ToolRunId)
            .Select(x => new { ToolRunId = x.Key, Count = x.Count() })
            .ToDictionaryAsync(x => x.ToolRunId, x => x.Count, cancellationToken);

        var responseItems = runs.Select(run => new ToolRunSummaryResponse(
            run.Id,
            run.ToolSlug,
            run.CreatedAtUtc,
            artifactCounts.GetValueOrDefault(run.Id, 0)))
            .ToList();

        return new ToolHistoryPageResponse(page, pageSize, total, responseItems);
    }
}
