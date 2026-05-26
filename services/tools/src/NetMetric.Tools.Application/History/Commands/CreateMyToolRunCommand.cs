// <copyright file="CreateMyToolRunCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Application.Abstractions.Security;
using NetMetric.Tools.Application.Abstractions.Storage;
using NetMetric.Tools.Application.Common;
using NetMetric.Tools.Contracts.History;
using NetMetric.Tools.Domain.Entities;
using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Application.History.Commands;

public sealed record CreateMyToolRunCommand(
    CreateToolRunRequest Request,
    Stream ArtifactStream,
    long ArtifactLength) : IRequest<CreateToolRunResponse>;

public sealed class CreateMyToolRunCommandHandler(
    IToolsDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor,
    IToolArtifactStorage artifactStorage,
    IToolsUploadSecurityValidator uploadSecurityValidator,
    IToolsFileSecurityScanner fileSecurityScanner) : IRequestHandler<CreateMyToolRunCommand, CreateToolRunResponse>
{
    public async Task<CreateToolRunResponse> Handle(CreateMyToolRunCommand request, CancellationToken cancellationToken)
    {
        var user = currentUserAccessor.GetRequired();
        var slug = new ToolSlug(request.Request.ToolSlug);

        var tool = await dbContext.ToolDefinitions.FirstOrDefaultAsync(x => x.Slug == slug.Value, cancellationToken)
            ?? throw new InvalidOperationException("Tool definition not found.");

        ArtifactRules.EnsureSaveAllowed(tool, request.Request.ArtifactMimeType, request.ArtifactLength);
        var validation = await uploadSecurityValidator.ValidateAsync(
            new ToolsUploadValidationRequest(
                request.Request.ArtifactMimeType,
                request.Request.ArtifactFileName,
                request.ArtifactLength,
                tool.AcceptedMimeTypes.ToArray(),
                request.ArtifactStream),
            cancellationToken);
        var scanResult = await fileSecurityScanner.ScanAsync(validation.SafeFileName, validation.DetectedMimeType, request.ArtifactStream, cancellationToken);
        if (!scanResult.IsSafe)
        {
            throw new InvalidOperationException($"Upload rejected by security scanner: {scanResult.Reason ?? "unknown"}");
        }

        var safeName = validation.SafeFileName;
        var storageKey = new StorageKey($"tools/{user.UserId:D}/{DateTimeOffset.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}-{safeName}");
        var checksum = validation.ChecksumSha256;

        var run = new ToolRun(OwnerUserId.From(user.UserId), slug, request.Request.InputSummaryJson);
        var artifact = new ToolArtifact(
            run.Id,
            OwnerUserId.From(user.UserId),
            new MimeType(validation.DetectedMimeType),
            new FileSizeBytes(request.ArtifactLength),
            storageKey,
            safeName,
            checksum);

        await artifactStorage.PutAsync(new ToolArtifactWriteRequest(storageKey.Value, artifact.MimeType, safeName, checksum, request.ArtifactStream, cancellationToken));

        await dbContext.ToolRuns.AddAsync(run, cancellationToken);
        await dbContext.ToolArtifacts.AddAsync(artifact, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateToolRunResponse(run.Id, artifact.Id, run.CreatedAtUtc);
    }
}
