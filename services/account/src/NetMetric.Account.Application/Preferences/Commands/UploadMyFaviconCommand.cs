// <copyright file="UploadMyFaviconCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;
using NetMetric.Account.Contracts.Profiles;
using NetMetric.Account.Domain.Common;
using NetMetric.Account.Domain.Preferences;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Clock;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Security;

namespace NetMetric.Account.Application.Preferences.Commands;

public sealed record UploadMyFaviconCommand(
    string FileName,
    string ContentType,
    Stream Content,
    long Length) : IRequest<Result<AvatarUploadResponse>>;

public sealed class UploadMyFaviconCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserPreference> preferences,
    IRepository<IAccountDbContext, AccountMediaAsset> mediaAssets,
    IAccountDbContext dbContext,
    IMediaStorageProvider storageProvider,
    IMediaUrlBuilder urlBuilder)
    : IRequestHandler<UploadMyFaviconCommand, Result<AvatarUploadResponse>>
{
    private const long MaxFaviconBytes = 1024 * 1024;

    public async Task<Result<AvatarUploadResponse>> Handle(UploadMyFaviconCommand command, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);

        var preference = await preferences.Query
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (preference is null)
        {
            preference = UserPreference.CreateDefault(tenantId, userId, clock.UtcNow);
            await preferences.AddAsync(preference, cancellationToken);
        }

        FaviconValidationResult validation;
        try
        {
            validation = await FaviconValidator.ValidateAsync(command.FileName, command.ContentType, command.Content, command.Length, MaxFaviconBytes, cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Result<AvatarUploadResponse>.Failure(Error.Validation(exception.Message));
        }

        using var uploadCopy = new MemoryStream();
        await command.Content.CopyToAsync(uploadCopy, cancellationToken);
        uploadCopy.Position = 0;
        var hash = await MediaHashing.ComputeSha256HexAsync(uploadCopy, cancellationToken);
        uploadCopy.Position = 0;

        var assetId = Guid.NewGuid();
        var storageKey = BuildStorageKey(currentUser.TenantId, assetId, validation.Extension);
        await storageProvider.SaveAsync(storageKey, uploadCopy, validation.ContentType, cancellationToken);

        var safeFileName = BuildSafeFileName(command.FileName, validation.Extension);
        var previousFaviconMediaAssetId = preference.FaviconMediaAssetId;
        var asset = AccountMediaAsset.CreateFavicon(
            tenantId,
            userId,
            safeFileName,
            safeFileName,
            validation.ContentType,
            validation.Extension,
            uploadCopy.Length,
            hash,
            validation.Width,
            validation.Height,
            storageProvider.Name,
            storageKey,
            urlBuilder.BuildPublicUrl(storageKey),
            clock.UtcNow);

        await mediaAssets.AddAsync(asset, cancellationToken);
        preference.AssignFavicon(asset.Id, asset.PublicUrl, clock.UtcNow);

        if (previousFaviconMediaAssetId.HasValue && previousFaviconMediaAssetId.Value != asset.Id)
        {
            var previousAsset = await mediaAssets.GetByIdAsync(previousFaviconMediaAssetId.Value, cancellationToken);
            previousAsset?.MarkPendingCleanup(clock.UtcNow);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AvatarUploadResponse>.Success(new AvatarUploadResponse(
            asset.Id,
            asset.PublicUrl,
            asset.ContentType,
            asset.SizeBytes,
            asset.Width,
            asset.Height,
            "ready",
            "favicon",
            asset.CreatedAtUtc));
    }

    private static string BuildStorageKey(Guid tenantId, Guid assetId, string extension)
        => $"media/{tenantId.ToString().ToLowerInvariant()}/favicon/{DateTime.UtcNow:yyyy}/{DateTime.UtcNow:MM}/{assetId}/original{extension}";

    private static string BuildSafeFileName(string fileName, string extension)
    {
        var candidate = Path.GetFileName(fileName.Replace('\\', '/'));
        var baseName = Path.GetFileNameWithoutExtension(candidate);
        var safeBaseName = new string((baseName ?? string.Empty)
            .Trim()
            .Select(character => character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.'
                ? character
                : '-')
            .ToArray()).Trim('-', '.', '_');

        if (string.IsNullOrWhiteSpace(safeBaseName))
        {
            safeBaseName = "favicon";
        }

        var safeFileName = $"{safeBaseName}{extension}";
        return safeFileName.Length <= 260 ? safeFileName : $"favicon{extension}";
    }
}

public sealed record RemoveMyFaviconCommand : IRequest<Result<bool>>;

public sealed class RemoveMyFaviconCommandHandler(
    ICurrentUserAccessor currentUserAccessor,
    IClock clock,
    IRepository<IAccountDbContext, UserPreference> preferences,
    IRepository<IAccountDbContext, AccountMediaAsset> mediaAssets,
    IAccountDbContext dbContext)
    : IRequestHandler<RemoveMyFaviconCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(RemoveMyFaviconCommand request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserAccessor.GetRequired();
        var tenantId = TenantId.From(currentUser.TenantId);
        var userId = UserId.From(currentUser.UserId);
        var preference = await preferences.Query
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.UserId == userId, cancellationToken);
        if (preference is null)
        {
            return Result<bool>.Success(true);
        }

        var removedMediaAssetId = preference.FaviconMediaAssetId;
        if (removedMediaAssetId.HasValue)
        {
            var asset = await mediaAssets.GetByIdAsync(removedMediaAssetId.Value, cancellationToken);
            asset?.MarkPendingCleanup(clock.UtcNow);
        }

        preference.ClearFavicon(clock.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}

internal sealed record FaviconValidationResult(string ContentType, string Extension, int? Width, int? Height);

internal static partial class FaviconValidator
{
    private static readonly Regex SvgDimensionRegex = CreateSvgDimensionRegex();
    private static readonly Regex SvgViewBoxRegex = CreateSvgViewBoxRegex();

    public static async Task<FaviconValidationResult> ValidateAsync(
        string fileName,
        string declaredContentType,
        Stream content,
        long length,
        long maxBytes,
        CancellationToken cancellationToken)
    {
        if (length <= 0 || length > maxBytes)
        {
            throw new InvalidOperationException("Favicon file size is invalid.");
        }

        var extension = Path.GetExtension(fileName)?.ToLowerInvariant() ?? string.Empty;
        if (extension is not ".ico" and not ".png" and not ".svg")
        {
            throw new InvalidOperationException("Favicon file extension must be ICO, PNG, or SVG.");
        }

        using var copy = new MemoryStream();
        await content.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;
        content.Position = 0;

        return extension switch
        {
            ".png" => ValidatePng(copy, declaredContentType),
            ".ico" => ValidateIco(copy, declaredContentType),
            ".svg" => ValidateSvg(copy, declaredContentType),
            _ => throw new InvalidOperationException("Unsupported favicon file type.")
        };
    }

    private static FaviconValidationResult ValidatePng(Stream content, string declaredContentType)
    {
        Span<byte> header = stackalloc byte[24];
        if (content.Read(header) < header.Length ||
            header[0] != 0x89 || header[1] != 0x50 || header[2] != 0x4E || header[3] != 0x47 ||
            header[4] != 0x0D || header[5] != 0x0A || header[6] != 0x1A || header[7] != 0x0A)
        {
            throw new InvalidOperationException("PNG favicon payload is invalid.");
        }

        EnsureDeclaredContentType(declaredContentType, "image/png");
        var width = ReadBigEndianInt32(header[16..20]);
        var height = ReadBigEndianInt32(header[20..24]);
        EnsureMaxDimensions(width, height);
        return new("image/png", ".png", width, height);
    }

    private static FaviconValidationResult ValidateIco(Stream content, string declaredContentType)
    {
        using var reader = new BinaryReader(content, Encoding.UTF8, leaveOpen: true);
        if (content.Length < 22 ||
            reader.ReadUInt16() != 0 ||
            reader.ReadUInt16() != 1)
        {
            throw new InvalidOperationException("ICO favicon payload is invalid.");
        }

        EnsureDeclaredContentType(declaredContentType, "image/x-icon", "image/vnd.microsoft.icon");
        var count = reader.ReadUInt16();
        if (count == 0)
        {
            throw new InvalidOperationException("ICO favicon payload is empty.");
        }

        var maxWidth = 0;
        var maxHeight = 0;
        for (var index = 0; index < count; index++)
        {
            var width = reader.ReadByte();
            var height = reader.ReadByte();
            _ = reader.ReadBytes(14);
            maxWidth = Math.Max(maxWidth, width == 0 ? 256 : width);
            maxHeight = Math.Max(maxHeight, height == 0 ? 256 : height);
        }

        EnsureMaxDimensions(maxWidth, maxHeight);
        return new("image/x-icon", ".ico", maxWidth, maxHeight);
    }

    private static FaviconValidationResult ValidateSvg(Stream content, string declaredContentType)
    {
        EnsureDeclaredContentType(declaredContentType, "image/svg+xml");
        using var reader = new StreamReader(content, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var svg = reader.ReadToEnd();
        if (!svg.Contains("<svg", StringComparison.OrdinalIgnoreCase) ||
            svg.Contains("<script", StringComparison.OrdinalIgnoreCase) ||
            svg.Contains("javascript:", StringComparison.OrdinalIgnoreCase) ||
            SvgEventAttributeRegex().IsMatch(svg))
        {
            throw new InvalidOperationException("SVG favicon payload is not allowed.");
        }

        var dimensions = TryReadSvgDimensions(svg);
        if (dimensions is null)
        {
            throw new InvalidOperationException("SVG favicon must include width/height or viewBox dimensions.");
        }

        EnsureMaxDimensions(dimensions.Value.Width, dimensions.Value.Height);
        return new("image/svg+xml", ".svg", dimensions.Value.Width, dimensions.Value.Height);
    }

    private static (int Width, int Height)? TryReadSvgDimensions(string svg)
    {
        var dimensionMatch = SvgDimensionRegex.Match(svg);
        if (dimensionMatch.Success &&
            TryParseSvgLength(dimensionMatch.Groups["width"].Value, out var width) &&
            TryParseSvgLength(dimensionMatch.Groups["height"].Value, out var height))
        {
            return (width, height);
        }

        var viewBoxMatch = SvgViewBoxRegex.Match(svg);
        if (viewBoxMatch.Success &&
            double.TryParse(viewBoxMatch.Groups["width"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var viewBoxWidth) &&
            double.TryParse(viewBoxMatch.Groups["height"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var viewBoxHeight))
        {
            return ((int)Math.Ceiling(viewBoxWidth), (int)Math.Ceiling(viewBoxHeight));
        }

        return null;
    }

    private static bool TryParseSvgLength(string value, out int length)
    {
        var match = SvgLengthRegex().Match(value);
        if (!match.Success ||
            !double.TryParse(match.Groups["value"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            length = 0;
            return false;
        }

        length = (int)Math.Ceiling(parsed);
        return true;
    }

    private static void EnsureDeclaredContentType(string declaredContentType, params string[] allowedContentTypes)
    {
        if (string.IsNullOrWhiteSpace(declaredContentType))
        {
            return;
        }

        if (!allowedContentTypes.Contains(declaredContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Declared favicon content type does not match file content.");
        }
    }

    private static void EnsureMaxDimensions(int width, int height)
    {
        if (width <= 0 || height <= 0 || width > 512 || height > 512)
        {
            throw new InvalidOperationException("Favicon dimensions must be at most 512x512.");
        }
    }

    private static int ReadBigEndianInt32(ReadOnlySpan<byte> bytes)
        => (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];

    [GeneratedRegex("""\bwidth\s*=\s*["'](?<width>[^"']+)["'][\s\S]*?\bheight\s*=\s*["'](?<height>[^"']+)["']""", RegexOptions.IgnoreCase)]
    private static partial Regex CreateSvgDimensionRegex();

    [GeneratedRegex("""\bviewBox\s*=\s*["']\s*[-\d.]+\s+[-\d.]+\s+(?<width>[-\d.]+)\s+(?<height>[-\d.]+)\s*["']""", RegexOptions.IgnoreCase)]
    private static partial Regex CreateSvgViewBoxRegex();

    [GeneratedRegex("""\bon[a-z]+\s*=""", RegexOptions.IgnoreCase)]
    private static partial Regex SvgEventAttributeRegex();

    [GeneratedRegex("""^(?<value>\d+(\.\d+)?)(px)?$""", RegexOptions.IgnoreCase)]
    private static partial Regex SvgLengthRegex();
}
