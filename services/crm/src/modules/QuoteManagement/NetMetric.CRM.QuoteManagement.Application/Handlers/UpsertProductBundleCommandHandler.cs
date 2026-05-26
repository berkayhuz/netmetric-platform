// <copyright file="UpsertProductBundleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class UpsertProductBundleCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertProductBundleCommand, ProductBundleDto>
{
    public async Task<ProductBundleDto> Handle(UpsertProductBundleCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = request.ProductBundleId.HasValue
            ? await dbContext.ProductBundles.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == request.ProductBundleId.Value, cancellationToken)
            : null;

        if (request.ProductBundleId.HasValue && entity is null)
            throw new KeyNotFoundException("Product bundle was not found.");

        entity ??= new ProductBundle
        {
            TenantId = currentUserService.TenantId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName
        };

        if (request.ProductBundleId.HasValue)
        {
            var expected = QuoteHandlerHelpers.ParseRowVersion(request.RowVersion);
            if (expected.Length > 0)
                entity.RowVersion = expected;
        }

        entity.Code = request.Code.Trim().ToUpperInvariant();
        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.Segment = string.IsNullOrWhiteSpace(request.Segment) ? null : request.Segment.Trim();
        entity.Industry = string.IsNullOrWhiteSpace(request.Industry) ? null : request.Industry.Trim();
        entity.DiscountRate = request.DiscountRate;
        entity.MinimumBudget = request.MinimumBudget;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        entity.Items.Clear();
        foreach (var item in request.Items)
        {
            entity.Items.Add(new ProductBundleItem
            {
                TenantId = currentUserService.TenantId,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                IsOptional = item.IsOptional,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserService.UserName,
                UpdatedBy = currentUserService.UserName
            });
        }

        if (!request.ProductBundleId.HasValue)
            await dbContext.ProductBundles.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
