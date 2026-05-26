// <copyright file="CreateCatalogProductCommandHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.ProductCatalog.Application.Features.Products.Commands.CreateCatalogProduct;

public sealed class CreateCatalogProductCommandHandler(
    IProductCatalogDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<CreateCatalogProductCommand, ProductCatalogSummaryDto>
{
    public async Task<ProductCatalogSummaryDto> Handle(CreateCatalogProductCommand request, CancellationToken cancellationToken)
    {
        var normalizedCode = request.Code.Trim();
        var tenantId = currentUserService.TenantId;

        var codeExists = await dbContext.Products
            .AnyAsync(
                x => x.TenantId == tenantId
                    && !x.IsDeleted
                    && x.Code == normalizedCode,
                cancellationToken);

        if (codeExists)
            throw new ConflictAppException("A catalog product with the same code already exists.");

        var entity = new CatalogProduct(
            request.Code,
            request.Name,
            request.Description,
            request.CategoryId,
            request.UnitPrice,
            request.CurrencyCode);
        await dbContext.Products.AddAsync(entity, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueCodeViolation(ex))
        {
            throw new ConflictAppException("A catalog product with the same code already exists.");
        }

        return new ProductCatalogSummaryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CategoryId = entity.CategoryId,
            UnitPrice = entity.UnitPrice,
            CurrencyCode = entity.CurrencyCode
        };
    }

    private static bool IsUniqueCodeViolation(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message;
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        return message.Contains("duplicate", StringComparison.OrdinalIgnoreCase)
            && message.Contains("IX_CatalogProduct_TenantId_Code", StringComparison.OrdinalIgnoreCase);
    }
}
