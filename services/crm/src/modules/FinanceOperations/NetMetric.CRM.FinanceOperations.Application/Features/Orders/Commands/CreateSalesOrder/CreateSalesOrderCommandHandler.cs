// <copyright file="CreateSalesOrderCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Contracts.DTOs;
using NetMetric.CRM.FinanceOperations.Domain.Entities.Orders;

namespace NetMetric.CRM.FinanceOperations.Application.Features.Orders.Commands.CreateSalesOrder;

public sealed class CreateSalesOrderCommandHandler(IFinanceOperationsDbContext dbContext)
    : IRequestHandler<CreateSalesOrderCommand, FinanceOperationsSummaryDto>
{
    public async Task<FinanceOperationsSummaryDto> Handle(CreateSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var entity = new SalesOrder(request.Code, request.Name, request.Description);
        await dbContext.Orders.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new FinanceOperationsSummaryDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Description = entity.Description,
            IsActive = entity.IsActive
        };
    }
}
