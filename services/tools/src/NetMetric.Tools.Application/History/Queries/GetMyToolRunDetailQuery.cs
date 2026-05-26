// <copyright file="GetMyToolRunDetailQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Contracts.History;

namespace NetMetric.Tools.Application.History.Queries;

public sealed record GetMyToolRunDetailQuery(Guid RunId) : IRequest<ToolRunDetailResponse?>;

public sealed class GetMyToolRunDetailQueryHandler(
    IToolsDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor) : IRequestHandler<GetMyToolRunDetailQuery, ToolRunDetailResponse?>
{
    public async Task<ToolRunDetailResponse?> Handle(GetMyToolRunDetailQuery request, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.GetRequired();

        var run = await dbContext.ToolRuns.AsNoTracking().FirstOrDefaultAsync(
            x => x.Id == request.RunId && x.OwnerUserId == user.UserId && x.DeletedAtUtc == null,
            cancellationToken);

        if (run is null)
        {
            return null;
        }

        var artifacts = await dbContext.ToolArtifacts
            .AsNoTracking()
            .Where(x => x.ToolRunId == run.Id && x.OwnerUserId == user.UserId && x.DeletedAtUtc == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new ToolArtifactResponse(
                x.Id,
                x.MimeType,
                x.SizeBytes,
                x.OriginalFileName,
                x.ChecksumSha256,
                x.CreatedAtUtc,
                x.ExpiresAtUtc))
            .ToListAsync(cancellationToken);

        return new ToolRunDetailResponse(run.Id, run.ToolSlug, run.InputSummaryJson, run.CreatedAtUtc, artifacts);
    }
}
