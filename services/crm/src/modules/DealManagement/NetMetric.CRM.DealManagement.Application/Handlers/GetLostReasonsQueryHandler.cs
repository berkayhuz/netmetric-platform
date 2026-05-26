// <copyright file="GetLostReasonsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Queries.Deals;
using NetMetric.CRM.DealManagement.Contracts.DTOs;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class GetLostReasonsQueryHandler(IDealManagementDbContext dbContext) : IRequestHandler<GetLostReasonsQuery, IReadOnlyList<LostReasonDto>>
{
    public async Task<IReadOnlyList<LostReasonDto>> Handle(GetLostReasonsQuery request, CancellationToken cancellationToken)
        => await dbContext.LostReasons.AsNoTracking().OrderBy(x => x.Name).Select(x => new LostReasonDto(x.Id, x.Name, x.Description, x.IsDefault)).ToListAsync(cancellationToken);
}
