// <copyright file="QuoteProductReadModelSyncService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Catalog;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Services;

public sealed class QuoteProductReadModelSyncService(
    QuoteManagementDbContext quoteDbContext,
    IProductCatalogDbContext productCatalogDbContext,
    ICurrentUserService currentUserService) : IQuoteProductReadModelSyncService
{
    public async Task SyncAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var requestedProductIds = productIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (requestedProductIds.Length == 0)
            return;

        var sourceProducts = await productCatalogDbContext.Products
            .Where(x => requestedProductIds.Contains(x.Id) && x.IsActive)
            .Select(x => new ProductProjection(
                x.Id,
                x.Code,
                x.Name,
                x.Description,
                x.IsActive))
            .ToListAsync(cancellationToken);

        var missingProductIds = requestedProductIds
            .Except(sourceProducts.Select(x => x.Id))
            .Select(x => x.ToString())
            .ToArray();

        if (missingProductIds.Length > 0)
        {
            throw new ValidationAppException(
                "One or more quote items reference unknown products.",
                new Dictionary<string, string[]>
                {
                    ["items"] = [$"Unknown product ids: {string.Join(", ", missingProductIds)}"]
                });
        }

        var localProducts = await quoteDbContext.Products
            .Where(x => requestedProductIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var utcNow = DateTime.UtcNow;
        var actor = currentUserService.UserName;

        foreach (var sourceProduct in sourceProducts)
        {
            if (localProducts.TryGetValue(sourceProduct.Id, out var localProduct))
            {
                localProduct.Code = sourceProduct.Code;
                localProduct.Name = sourceProduct.Name;
                localProduct.Description = sourceProduct.Description;
                localProduct.Activate();
                localProduct.TenantId = tenantId;
                localProduct.UpdatedAt = utcNow;
                localProduct.UpdatedBy = actor;
                continue;
            }

            localProduct = new Product
            {
                TenantId = tenantId,
                Code = sourceProduct.Code,
                Name = sourceProduct.Name,
                Description = sourceProduct.Description,
                Price = 0,
                ProductCategoryId = null,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                CreatedBy = actor,
                UpdatedBy = actor
            };
            localProduct.Activate();

            var entry = quoteDbContext.Products.Add(localProduct);
            entry.Property(x => x.Id).CurrentValue = sourceProduct.Id;
        }
    }

    private sealed record ProductProjection(Guid Id, string Code, string Name, string? Description, bool IsActive);
}
