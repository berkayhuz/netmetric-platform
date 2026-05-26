// <copyright file="AssignOpportunityOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class AssignOpportunityOwnerCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<AssignOpportunityOwnerCommand>
{
    public async Task Handle(AssignOpportunityOwnerCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        entity.OwnerUserId = request.OwnerUserId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await outbox.EnqueueOpportunityUpdatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
