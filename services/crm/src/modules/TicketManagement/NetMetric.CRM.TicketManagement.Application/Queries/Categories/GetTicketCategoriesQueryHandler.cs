// <copyright file="GetTicketCategoriesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Common;
using NetMetric.CRM.TicketManagement.Contracts.DTOs;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.TicketManagement.Application.Queries.Categories;

public sealed class GetTicketCategoriesQueryHandler(
    ITicketManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<GetTicketCategoriesQuery, IReadOnlyList<TicketCategoryDto>>
{
    public async Task<IReadOnlyList<TicketCategoryDto>> Handle(GetTicketCategoriesQuery request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.TenantId;

        var query = dbContext.TicketCategories
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId);

        if (!request.IncludeInactive)
            query = query.Where(x => x.IsActive);

        return await query
            .OrderBy(x => x.Name)
            .Select(x => x.ToDto())
            .ToListAsync(cancellationToken);
    }
}
