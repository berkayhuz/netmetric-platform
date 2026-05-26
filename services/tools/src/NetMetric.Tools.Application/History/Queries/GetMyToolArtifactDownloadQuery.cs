// <copyright file="GetMyToolArtifactDownloadQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Application.Abstractions.Storage;

namespace NetMetric.Tools.Application.History.Queries;

public sealed record GetMyToolArtifactDownloadQuery(Guid RunId) : IRequest<(Stream Content, string MimeType, string FileName)?>;

public sealed class GetMyToolArtifactDownloadQueryHandler(
    IToolsDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor,
    IToolArtifactStorage artifactStorage) : IRequestHandler<GetMyToolArtifactDownloadQuery, (Stream Content, string MimeType, string FileName)?>
{
    public async Task<(Stream Content, string MimeType, string FileName)?> Handle(GetMyToolArtifactDownloadQuery request, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.GetRequired();

        var artifact = await dbContext.ToolArtifacts
            .AsNoTracking()
            .Where(x => x.ToolRunId == request.RunId && x.OwnerUserId == user.UserId && x.DeletedAtUtc == null)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (artifact is null)
        {
            return null;
        }

        var stored = await artifactStorage.GetAsync(artifact.StorageKey, cancellationToken);
        if (stored is null)
        {
            return null;
        }

        return (stored.Content, artifact.MimeType, artifact.OriginalFileName);
    }
}
