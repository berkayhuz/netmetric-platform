// <copyright file="GetSlaPoliciesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Common;
using NetMetric.CRM.TicketSlaManagement.Contracts.DTOs;

namespace NetMetric.CRM.TicketSlaManagement.Application.Queries.SlaPolicies;

public sealed class GetSlaPoliciesQueryHandler(ITicketSlaManagementDbContext dbContext) : IRequestHandler<GetSlaPoliciesQuery, IReadOnlyList<SlaPolicyListItemDto>>
{
    public async Task<IReadOnlyList<SlaPolicyListItemDto>> Handle(GetSlaPoliciesQuery request, CancellationToken cancellationToken) =>
        await dbContext.SlaPolicies
            .AsNoTracking()
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
}
