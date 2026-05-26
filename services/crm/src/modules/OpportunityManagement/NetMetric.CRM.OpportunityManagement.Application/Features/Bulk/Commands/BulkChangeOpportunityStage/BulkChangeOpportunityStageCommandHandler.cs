// <copyright file="BulkChangeOpportunityStageCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Bulk.Commands.BulkChangeOpportunityStage;

public sealed class BulkChangeOpportunityStageCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<BulkChangeOpportunityStageCommand, int>
{
    public async Task<int> Handle(BulkChangeOpportunityStageCommand request, CancellationToken cancellationToken)
    {
        var items = await dbContext.Opportunities.Where(x => x.TenantId == currentUserService.TenantId && request.OpportunityIds.Contains(x.Id)).ToListAsync(cancellationToken);
        foreach (var item in items)
        {
            item.Stage = request.NewStage;
            item.Status = request.NewStage switch
            {
                OpportunityStageType.Won => OpportunityStatusType.Won,
                OpportunityStageType.Lost => OpportunityStatusType.Lost,
                _ => OpportunityStatusType.Open
            };
            item.UpdatedAt = DateTime.UtcNow;
            item.UpdatedBy = currentUserService.UserName;
            await outbox.EnqueueOpportunityUpdatedAsync(item, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return items.Count;
    }
}
