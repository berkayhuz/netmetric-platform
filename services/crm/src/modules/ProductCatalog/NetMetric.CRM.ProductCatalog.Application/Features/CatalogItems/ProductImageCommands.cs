// <copyright file="ProductImageCommands.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;
using NetMetric.CRM.ProductCatalog.Domain.Entities.Products;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Media.Abstractions;
using NetMetric.Media.Models;

namespace NetMetric.CRM.ProductCatalog.Application.Features.CatalogItems;

public sealed record UploadProductImageCommand(Guid ProductId, string FileName, string ContentType, Stream Content, long Length, bool IsPrimary, int SortOrder, string? AltText) : IRequest<ProductImageDto>;
public sealed record ListProductImagesQuery(Guid ProductId) : IRequest<IReadOnlyList<ProductImageDto>>;
public sealed record SetPrimaryProductImageCommand(Guid ProductId, Guid ProductImageId) : IRequest;
public sealed record RemoveProductImageCommand(Guid ProductId, Guid ProductImageId) : IRequest;
public sealed record UploadCategoryImageCommand(Guid CategoryId, string FileName, string ContentType, Stream Content, long Length) : IRequest<string>;
public sealed record RemoveCategoryImageCommand(Guid CategoryId) : IRequest;

public sealed class ProductImageHandler(
    IProductCatalogDbContext dbContext,
    ICurrentUserService currentUserService,
    IMediaAssetService mediaAssetService) :
    IRequestHandler<UploadProductImageCommand, ProductImageDto>,
    IRequestHandler<ListProductImagesQuery, IReadOnlyList<ProductImageDto>>,
    IRequestHandler<SetPrimaryProductImageCommand>,
    IRequestHandler<RemoveProductImageCommand>,
    IRequestHandler<UploadCategoryImageCommand, string>,
    IRequestHandler<RemoveCategoryImageCommand>
{
    private const int MaxProductImageCount = 10;

    public async Task<ProductImageDto> Handle(UploadProductImageCommand request, CancellationToken cancellationToken)
    {
        var product = await dbContext.Products.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == request.ProductId, cancellationToken);
        var existingImageCount = await dbContext.ProductImages
            .Where(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.ProductId == request.ProductId)
            .CountAsync(cancellationToken);

        if (existingImageCount >= MaxProductImageCount)
        {
            throw new ValidationAppException($"A product can have at most {MaxProductImageCount} images.");
        }

        MediaUploadResult upload;
        try
        {
            upload = await mediaAssetService.UploadImageAsync(
                new MediaUploadRequest(
                    currentUserService.TenantId.ToString(),
                    "product-image",
                    currentUserService.UserId.ToString(),
                    request.FileName,
                    request.ContentType,
                    request.Content,
                    request.Length,
                    "product-catalog"),
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationAppException(ex.Message);
        }

        var asset = ProductCatalogMediaAsset.Create("product-image", request.FileName, upload.ContentType, upload.Extension, upload.SizeBytes, upload.Sha256Hash, upload.Width, upload.Height, upload.StorageProvider, upload.StorageKey, upload.PublicUrl);
        await dbContext.MediaAssets.AddAsync(asset, cancellationToken);
        if (request.IsPrimary)
        {
            var existingPrimary = await dbContext.ProductImages.Where(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.ProductId == request.ProductId && x.IsPrimary).ToListAsync(cancellationToken);
            foreach (var image in existingPrimary) image.SetPrimary(false);
            product.SetPrimaryImage(asset.Id, asset.PublicUrl);
        }

        var productImage = new ProductImage(request.ProductId, asset.Id, request.SortOrder, request.IsPrimary, request.AltText);
        await dbContext.ProductImages.AddAsync(productImage, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return new ProductImageDto { Id = productImage.Id, ProductId = request.ProductId, MediaAssetId = asset.Id, PublicUrl = asset.PublicUrl, SortOrder = productImage.SortOrder, IsPrimary = productImage.IsPrimary, AltText = productImage.AltText };
    }

    public async Task<IReadOnlyList<ProductImageDto>> Handle(ListProductImagesQuery request, CancellationToken cancellationToken)
        => await dbContext.ProductImages
            .Where(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.ProductId == request.ProductId)
            .Join(dbContext.MediaAssets, image => image.MediaAssetId, media => media.Id, (image, media) => new ProductImageDto { Id = image.Id, ProductId = image.ProductId, MediaAssetId = image.MediaAssetId, PublicUrl = media.PublicUrl, SortOrder = image.SortOrder, IsPrimary = image.IsPrimary, AltText = image.AltText })
            .OrderBy(x => x.SortOrder).ToListAsync(cancellationToken);

    public async Task Handle(SetPrimaryProductImageCommand request, CancellationToken cancellationToken)
    {
        var images = await dbContext.ProductImages.Where(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.ProductId == request.ProductId).ToListAsync(cancellationToken);
        foreach (var image in images) image.SetPrimary(image.Id == request.ProductImageId);
        var selected = images.Single(x => x.Id == request.ProductImageId);
        var asset = await dbContext.MediaAssets.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == selected.MediaAssetId, cancellationToken);
        var product = await dbContext.Products.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == request.ProductId, cancellationToken);
        product.SetPrimaryImage(asset.Id, asset.PublicUrl);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task Handle(RemoveProductImageCommand request, CancellationToken cancellationToken)
    {
        var image = await dbContext.ProductImages.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.ProductId == request.ProductId && x.Id == request.ProductImageId, cancellationToken);
        var asset = await dbContext.MediaAssets.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == image.MediaAssetId, cancellationToken);
        await mediaAssetService.DeleteAsync(asset.StorageKey, cancellationToken);
        asset.MarkDeleted();
        dbContext.ProductImages.Remove(image);
        var product = await dbContext.Products.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == request.ProductId, cancellationToken);
        if (product.PrimaryImageMediaAssetId == asset.Id) product.SetPrimaryImage(null, null);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> Handle(UploadCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == request.CategoryId, cancellationToken);
        if (category.ImageMediaAssetId.HasValue)
        {
            throw new ValidationAppException("A category can have at most 1 image. Remove the current image before uploading a new one.");
        }

        MediaUploadResult upload;
        try
        {
            upload = await mediaAssetService.UploadImageAsync(
                new MediaUploadRequest(
                    currentUserService.TenantId.ToString(),
                    "category-image",
                    currentUserService.UserId.ToString(),
                    request.FileName,
                    request.ContentType,
                    request.Content,
                    request.Length,
                    "product-catalog"),
                cancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationAppException(ex.Message);
        }

        var asset = ProductCatalogMediaAsset.Create("category-image", request.FileName, upload.ContentType, upload.Extension, upload.SizeBytes, upload.Sha256Hash, upload.Width, upload.Height, upload.StorageProvider, upload.StorageKey, upload.PublicUrl);
        await dbContext.MediaAssets.AddAsync(asset, cancellationToken);
        category.SetImage(asset.Id, asset.PublicUrl);
        await dbContext.SaveChangesAsync(cancellationToken);
        return asset.PublicUrl;
    }

    public async Task Handle(RemoveCategoryImageCommand request, CancellationToken cancellationToken)
    {
        var category = await dbContext.Categories.SingleAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == request.CategoryId, cancellationToken);
        if (category.ImageMediaAssetId.HasValue)
        {
            var asset = await dbContext.MediaAssets.SingleOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && !x.IsDeleted && x.Id == category.ImageMediaAssetId.Value, cancellationToken);
            if (asset is not null) { await mediaAssetService.DeleteAsync(asset.StorageKey, cancellationToken); asset.MarkDeleted(); }
        }
        category.SetImage(null, null);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
