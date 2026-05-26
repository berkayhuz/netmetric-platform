// <copyright file="AccountMediaAsset.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Domain.Common;

namespace NetMetric.Account.Domain.Profiles;

public sealed class AccountMediaAsset
{
    private AccountMediaAsset()
    {
        Module = string.Empty;
        Purpose = string.Empty;
        OriginalFileName = string.Empty;
        SafeFileName = string.Empty;
        ContentType = string.Empty;
        Extension = string.Empty;
        Sha256Hash = string.Empty;
        StorageProvider = string.Empty;
        StorageKey = string.Empty;
        PublicUrl = string.Empty;
        Status = "ready";
        Version = [];
    }

    public Guid Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public UserId? OwnerUserId { get; private set; }
    public string Module { get; private set; }
    public string Purpose { get; private set; }
    public string OriginalFileName { get; private set; }
    public string SafeFileName { get; private set; }
    public string ContentType { get; private set; }
    public string Extension { get; private set; }
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string StorageProvider { get; private set; }
    public string StorageKey { get; private set; }
    public string PublicUrl { get; private set; }
    public string Visibility { get; private set; } = "public";
    public string Status { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public UserId? CreatedByUserId { get; private set; }
    public byte[] Version { get; private set; }

    public static AccountMediaAsset CreateAvatar(
        TenantId tenantId,
        UserId ownerUserId,
        string originalFileName,
        string safeFileName,
        string contentType,
        string extension,
        long sizeBytes,
        string sha256Hash,
        int? width,
        int? height,
        string storageProvider,
        string storageKey,
        string publicUrl,
        DateTimeOffset utcNow)
    {
        return new AccountMediaAsset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            Module = "account",
            Purpose = "avatar",
            OriginalFileName = NormalizeRequired(originalFileName, 260),
            SafeFileName = NormalizeRequired(safeFileName, 260),
            ContentType = NormalizeRequired(contentType, 128),
            Extension = NormalizeRequired(extension, 16),
            SizeBytes = sizeBytes,
            Sha256Hash = NormalizeRequired(sha256Hash, 128),
            Width = width,
            Height = height,
            StorageProvider = NormalizeRequired(storageProvider, 64),
            StorageKey = NormalizeRequired(storageKey, 512),
            PublicUrl = NormalizeRequired(publicUrl, 2048),
            Visibility = "public",
            Status = "ready",
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            CreatedByUserId = ownerUserId,
            Version = []
        };
    }

    public static AccountMediaAsset CreateFavicon(
        TenantId tenantId,
        UserId ownerUserId,
        string originalFileName,
        string safeFileName,
        string contentType,
        string extension,
        long sizeBytes,
        string sha256Hash,
        int? width,
        int? height,
        string storageProvider,
        string storageKey,
        string publicUrl,
        DateTimeOffset utcNow)
    {
        return new AccountMediaAsset
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            Module = "account",
            Purpose = "favicon",
            OriginalFileName = NormalizeRequired(originalFileName, 260),
            SafeFileName = NormalizeRequired(safeFileName, 260),
            ContentType = NormalizeRequired(contentType, 128),
            Extension = NormalizeRequired(extension, 16),
            SizeBytes = sizeBytes,
            Sha256Hash = NormalizeRequired(sha256Hash, 128),
            Width = width,
            Height = height,
            StorageProvider = NormalizeRequired(storageProvider, 64),
            StorageKey = NormalizeRequired(storageKey, 512),
            PublicUrl = NormalizeRequired(publicUrl, 2048),
            Visibility = "public",
            Status = "ready",
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow,
            CreatedByUserId = ownerUserId,
            Version = []
        };
    }

    public void MarkDeleted(DateTimeOffset utcNow)
    {
        DeletedAtUtc = utcNow;
        Status = "deleted";
        UpdatedAtUtc = utcNow;
    }

    public void MarkPendingCleanup(DateTimeOffset utcNow)
    {
        DeletedAtUtc = utcNow;
        Status = "cleanup_pending";
        UpdatedAtUtc = utcNow;
    }

    private static string NormalizeRequired(string value, int max)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0 || normalized.Length > max)
        {
            throw new DomainValidationException("Invalid media field.");
        }

        return normalized;
    }
}
