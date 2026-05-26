// <copyright file="BulkAssignDealsOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class BulkAssignDealsOwnerCommandHandler(IDealManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<BulkAssignDealsOwnerCommand, int>
{
    public async Task<int> Handle(BulkAssignDealsOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var deals = await dbContext.Deals.Where(x => request.DealIds.Contains(x.Id)).ToListAsync(cancellationToken);
        foreach (var entity in deals)
        {
            entity.OwnerUserId = request.OwnerUserId;
            entity.UpdatedAt = DateTime.UtcNow;
            entity.UpdatedBy = currentUserService.UserName;
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return deals.Count;
    }
}
