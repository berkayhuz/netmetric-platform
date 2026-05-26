// <copyright file="ProductCatalogMediaAsset.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.ProductCatalog.Domain.Entities.Products;

public sealed class ProductCatalogMediaAsset : AuditableEntity
{
    public string Module { get; private set; } = null!;
    public string Purpose { get; private set; } = null!;
    public string OriginalFileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public string Extension { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public string Sha256Hash { get; private set; } = null!;
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public string StorageProvider { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public string PublicUrl { get; private set; } = null!;
    public string Visibility { get; private set; } = "public";
    public string Status { get; private set; } = "ready";

    private ProductCatalogMediaAsset() { }

    public static ProductCatalogMediaAsset Create(
        string purpose,
        string originalFileName,
        string contentType,
        string extension,
        long sizeBytes,
        string sha256Hash,
        int? width,
        int? height,
        string storageProvider,
        string storageKey,
        string publicUrl)
    {
        return new ProductCatalogMediaAsset
        {
            Module = "product-catalog",
            Purpose = purpose.Trim(),
            OriginalFileName = originalFileName.Trim(),
            ContentType = contentType.Trim(),
            Extension = extension.Trim(),
            SizeBytes = sizeBytes,
            Sha256Hash = sha256Hash.Trim(),
            Width = width,
            Height = height,
            StorageProvider = storageProvider.Trim(),
            StorageKey = storageKey.Trim(),
            PublicUrl = publicUrl.Trim()
        };
    }

    public void MarkDeleted()
    {
        Status = "deleted";
    }
}
