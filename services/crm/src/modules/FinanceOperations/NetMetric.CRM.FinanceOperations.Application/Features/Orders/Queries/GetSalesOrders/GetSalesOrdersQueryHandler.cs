// <copyright file="GetSalesOrdersQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Contracts.DTOs;

namespace NetMetric.CRM.FinanceOperations.Application.Features.Orders.Queries.GetSalesOrders;

public sealed class GetSalesOrdersQueryHandler(IFinanceOperationsDbContext dbContext)
    : IRequestHandler<GetSalesOrdersQuery, IReadOnlyList<FinanceOperationsSummaryDto>>
{
    public async Task<IReadOnlyList<FinanceOperationsSummaryDto>> Handle(
        GetSalesOrdersQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Orders
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new FinanceOperationsSummaryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                IsActive = x.IsActive
            })
            .ToListAsync(cancellationToken);
    }
}
