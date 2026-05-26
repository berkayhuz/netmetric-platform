// <copyright file="GetContractRecordByIdQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.ContractLifecycle.Contracts.DTOs;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Queries.GetContractRecordById;

public sealed class GetContractRecordByIdQueryHandler(IContractLifecycleDbContext dbContext)
    : IRequestHandler<GetContractRecordByIdQuery, ContractLifecycleSummaryDto?>
{
    public async Task<ContractLifecycleSummaryDto?> Handle(GetContractRecordByIdQuery request, CancellationToken cancellationToken)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ContractLifecycleSummaryDto
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
