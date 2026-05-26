// <copyright file="DeleteMyToolRunCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Application.Abstractions.Storage;

namespace NetMetric.Tools.Application.History.Commands;

public sealed record DeleteMyToolRunCommand(Guid RunId) : IRequest<bool>;

public sealed class DeleteMyToolRunCommandHandler(
    IToolsDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor,
    IToolArtifactStorage artifactStorage) : IRequestHandler<DeleteMyToolRunCommand, bool>
{
    public async Task<bool> Handle(DeleteMyToolRunCommand request, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.GetRequired();

        var run = await dbContext.ToolRuns
            .FirstOrDefaultAsync(x => x.Id == request.RunId && x.OwnerUserId == user.UserId && x.DeletedAtUtc == null, cancellationToken);

        if (run is null)
        {
            return false;
        }

        var artifacts = await dbContext.ToolArtifacts
            .Where(x => x.ToolRunId == run.Id && x.OwnerUserId == user.UserId && x.DeletedAtUtc == null)
            .ToListAsync(cancellationToken);

        run.SoftDelete();

        foreach (var artifact in artifacts)
        {
            artifact.SoftDelete();
            await artifactStorage.DeleteAsync(artifact.StorageKey, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
