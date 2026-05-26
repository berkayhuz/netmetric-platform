// <copyright file="ProductCatalogTrashRestoreService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Services;

public sealed class ProductCatalogTrashRestoreService(IProductCatalogDbContext dbContext)
    : IGlobalTrashProductCatalogRestoreService
{
    private readonly IProductCatalogDbContext _dbContext = dbContext;

    public async Task RestoreCatalogProductFromTrashAsync(GlobalTrashItem trashItem, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(trashItem.EntityType, CrmTrashEntityTypes.ProductCatalogItem, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestAppException("Trash item is not a catalog product.");
        }

        var product = await _dbContext.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Catalog product not found.");

        if (!product.IsDeleted)
        {
            throw new ConflictAppException("Catalog product is already active.");
        }

        if (product.CategoryId.HasValue)
        {
            var category = await _dbContext.Categories
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    x => x.Id == product.CategoryId.Value && x.TenantId == product.TenantId,
                    cancellationToken);

            if (category is null || category.IsDeleted)
            {
                throw new ConflictAppException("Catalog product cannot be restored because its category dependency is unavailable.");
            }
        }

        product.IsDeleted = false;
        product.DeletedAt = null;
        product.DeletedBy = null;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
