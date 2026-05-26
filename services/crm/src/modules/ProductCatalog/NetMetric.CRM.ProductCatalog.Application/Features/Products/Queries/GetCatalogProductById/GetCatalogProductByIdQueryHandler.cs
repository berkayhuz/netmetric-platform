// <copyright file="GetCatalogProductByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ProductCatalog.Application.Abstractions.Persistence;
using NetMetric.CRM.ProductCatalog.Contracts.DTOs;

namespace NetMetric.CRM.ProductCatalog.Application.Features.Products.Queries.GetCatalogProductById;

public sealed class GetCatalogProductByIdQueryHandler(IProductCatalogDbContext dbContext)
    : IRequestHandler<GetCatalogProductByIdQuery, ProductCatalogSummaryDto?>
{
    public async Task<ProductCatalogSummaryDto?> Handle(GetCatalogProductByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Products
            .Where(x => x.Id == request.Id)
            .Select(x => new ProductCatalogSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive,
                CategoryId = x.CategoryId,
                UnitPrice = x.UnitPrice,
                CurrencyCode = x.CurrencyCode
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
