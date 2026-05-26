// <copyright file="GetLostReasonsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.LostReasons.Queries.GetLostReasons;

public sealed class GetLostReasonsQueryHandler(IOpportunityManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<GetLostReasonsQuery, IReadOnlyList<LostReasonDto>>
{
    public async Task<IReadOnlyList<LostReasonDto>> Handle(GetLostReasonsQuery request, CancellationToken cancellationToken)
    {
        var items = await dbContext.LostReasons.AsNoTracking().Where(x => x.TenantId == currentUserService.TenantId).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        return items.Select(x => x.ToDto()).ToList();
    }
}
