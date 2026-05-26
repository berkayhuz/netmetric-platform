// <copyright file="ProductCatalogTrashPurgeService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;

namespace NetMetric.CRM.ProductCatalog.Infrastructure.Services;

public sealed class ProductCatalogTrashPurgeService(IProductCatalogDbContext dbContext)
    : IGlobalTrashProductCatalogPurgeService
{
    private readonly IProductCatalogDbContext _dbContext = dbContext;

    public async Task<int> PurgeCatalogProductFromTrashAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(trashItem.EntityType, CrmTrashEntityTypes.ProductCatalogItem, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var product = await _dbContext.Products
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == trashItem.EntityId && x.TenantId == trashItem.TenantId,
                cancellationToken);

        if (product is null)
        {
            trashItem.Status = CrmTrashStatuses.Purged;
            trashItem.PurgedAtUtc = nowUtc;
            return 1;
        }

        if (!product.IsDeleted)
        {
            return 0;
        }

        try
        {
            var deletedRows = await _dbContext.Products
                .IgnoreQueryFilters()
                .Where(x =>
                    x.Id == product.Id
                    && x.TenantId == product.TenantId
                    && x.IsDeleted)
                .ExecuteDeleteAsync(cancellationToken);

            if (deletedRows < 1)
            {
                return 0;
            }
        }
        catch (DbUpdateException)
        {
            return 0;
        }

        trashItem.Status = CrmTrashStatuses.Purged;
        trashItem.PurgedAtUtc = nowUtc;
        return 1;
    }
}
