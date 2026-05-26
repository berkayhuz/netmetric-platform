// <copyright file="AddOpportunityProductCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class AddOpportunityProductCommandHandler(IOpportunityManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<AddOpportunityProductCommand, OpportunityProductDto>
{
    public async Task<OpportunityProductDto> Handle(AddOpportunityProductCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Opportunities.AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken);
        if (!exists) throw new NotFoundAppException("Opportunity not found.");

        var entity = new OpportunityProduct
        {
            TenantId = currentUserService.TenantId,
            OpportunityId = request.OpportunityId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            DiscountRate = request.DiscountRate,
            VatRate = request.VatRate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await dbContext.OpportunityProducts.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
