// <copyright file="GetContractRecordsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.ContractLifecycle.Contracts.DTOs;

namespace NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Queries.GetContractRecords;

public sealed class GetContractRecordsQueryHandler(IContractLifecycleDbContext dbContext)
    : IRequestHandler<GetContractRecordsQuery, IReadOnlyList<ContractLifecycleSummaryDto>>
{
    public async Task<IReadOnlyList<ContractLifecycleSummaryDto>> Handle(
        GetContractRecordsQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ContractLifecycleSummaryDto
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
