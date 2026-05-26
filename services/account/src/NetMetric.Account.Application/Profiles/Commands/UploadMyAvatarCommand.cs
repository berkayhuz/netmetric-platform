// <copyright file="UploadMyAvatarCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Account.Application.Abstractions.Outbox;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Profiles;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Clock;
using NetMetric.Media;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;

namespace NetMetric.Account.Application.Profiles.Commands;

public sealed record UploadMyAvatarCommand(
    string FileName,
    string ContentType,
    Stream Content,
    long Length) : IRequest<Result<AvatarUploadResponse>>;

public sealed class UploadMyAvatarCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserProfile> profiles,
    IRepository<IAccountDbContext, AccountMediaAsset> mediaAssets,
    IAccountDbContext dbContext,
    IMediaAssetService mediaAssetService,
    IAccountOutboxWriter outboxWriter)
    : IRequestHandler<UploadMyAvatarCommand, Result<AvatarUploadResponse>>
{
    public async Task<Result<AvatarUploadResponse>> Handle(UploadMyAvatarCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);
        var profile = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result<AvatarUploadResponse>.Failure(Error.NotFound("Profile"));
        }

        MediaUploadResult upload;
        try
        {
            upload = await mediaAssetService.UploadImageAsync(
                new MediaUploadRequest(
                    currentUser.TenantId.ToString(),
                    "avatar",
                    currentUser.UserId.ToString(),
                    command.FileName,
                    command.ContentType,
                    command.Content,
                    command.Length,
                    "account"),
                cancellationToken);
        }
        catch (MediaValidationException exception)
        {
            return Result<AvatarUploadResponse>.Failure(Error.Validation(exception.Message));
        }

        var safeFileName = BuildSafeFileName(command.FileName, upload.Extension);
        var previousAvatarMediaAssetId = profile.AvatarMediaAssetId;
        var asset = AccountMediaAsset.CreateAvatar(
            tenantId,
            userId,
            safeFileName,
            safeFileName,
            upload.ContentType,
            upload.Extension,
            upload.SizeBytes,
            upload.Sha256Hash,
            upload.Width,
            upload.Height,
            upload.StorageProvider,
            upload.StorageKey,
            upload.PublicUrl,
            clock.UtcNow);
        await mediaAssets.AddAsync(asset, cancellationToken);
        profile.AssignManagedAvatar(asset.Id, upload.PublicUrl, clock.UtcNow);
        if (previousAvatarMediaAssetId.HasValue && previousAvatarMediaAssetId.Value != asset.Id)
        {
            var previousAsset = await mediaAssets.GetByIdAsync(previousAvatarMediaAssetId.Value, cancellationToken);
            previousAsset?.MarkPendingCleanup(clock.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await outboxWriter.EnqueueAsync(
            currentUser.TenantId,
            AccountOutboxEventTypes.AvatarChanged,
            new AccountAvatarChangedEvent(
                1,
                currentUser.TenantId,
                currentUser.UserId,
                currentUser.CorrelationId,
                clock.UtcNow,
                asset.Id,
                asset.ContentType,
                asset.SizeBytes,
                previousAvatarMediaAssetId),
            currentUser.CorrelationId,
            cancellationToken);

        return Result<AvatarUploadResponse>.Success(new AvatarUploadResponse(
            asset.Id,
            asset.PublicUrl,
            asset.ContentType,
            asset.SizeBytes,
            asset.Width,
            asset.Height,
            "ready",
            "avatar",
            asset.CreatedAtUtc));
    }

    private static string BuildSafeFileName(string fileName, string extension)
    {
        var candidate = Path.GetFileName(fileName.Replace('\\', '/'));
        var baseName = Path.GetFileNameWithoutExtension(candidate);
        var safeBaseName = SanitizeFileNameSegment(baseName);
        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "avatar";
        }

        var safeFileName = $"{safeBaseName}{extension}";
        return safeFileName.Length <= 260 ? safeFileName : $"avatar{extension}";
    }

    private static string SanitizeFileNameSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var chars = value
            .Trim()
            .Select(character => IsSafeFileNameCharacter(character) ? character : '-')
            .ToArray();

        return new string(chars).Trim('-', '.', '_');
    }

    private static bool IsSafeFileNameCharacter(char value) =>
        value is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.';
}

public sealed record RemoveMyAvatarCommand : IRequest<Result<bool>>;

public sealed class RemoveMyAvatarCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserProfile> profiles,
    IRepository<IAccountDbContext, AccountMediaAsset> mediaAssets,
    IAccountDbContext dbContext,
    IAccountOutboxWriter outboxWriter)
    : IRequestHandler<RemoveMyAvatarCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveMyAvatarCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);
        var profile = await profiles.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (profile is null)
        {
            return Result<bool>.Failure(Error.NotFound("Profile"));
        }

        var removedMediaAssetId = profile.AvatarMediaAssetId;
        if (removedMediaAssetId.HasValue)
        {
            var asset = await mediaAssets.GetByIdAsync(removedMediaAssetId.Value, cancellationToken);
            if (asset is not null)
            {
                asset.MarkPendingCleanup(clock.UtcNow);
            }
        }

        profile.ClearAvatar(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        await outboxWriter.EnqueueAsync(
            currentUser.TenantId,
            AccountOutboxEventTypes.AvatarDeleted,
            new AccountAvatarDeletedEvent(
                1,
                currentUser.TenantId,
                currentUser.UserId,
                currentUser.CorrelationId,
                clock.UtcNow,
                removedMediaAssetId),
            currentUser.CorrelationId,
            cancellationToken);

        return Result<bool>.Success(true);
    }
}
