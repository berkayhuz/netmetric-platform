// <copyright file="GetSalesOrderByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Contracts.DTOs;

namespace NetMetric.CRM.FinanceOperations.Application.Features.Orders.Queries.GetSalesOrderById;

public sealed class GetSalesOrderByIdQueryHandler(IFinanceOperationsDbContext dbContext)
    : IRequestHandler<GetSalesOrderByIdQuery, FinanceOperationsSummaryDto?>
{
    public async Task<FinanceOperationsSummaryDto?> Handle(GetSalesOrderByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .Where(x => x.Id == request.Id)
            .Select(x => new FinanceOperationsSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);
    }
}
