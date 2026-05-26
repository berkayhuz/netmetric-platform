// <copyright file="SoftDeleteOpportunityCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class SoftDeleteOpportunityCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<SoftDeleteOpportunityCommand>
{
    public async Task Handle(SoftDeleteOpportunityCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        await AddOpportunityTrashItemIfMissingAsync(opportunity, cancellationToken);
        opportunity.Deactivate();
        await outbox.EnqueueOpportunityDeletedAsync(opportunity, cancellationToken);
        dbContext.Opportunities.Remove(opportunity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddOpportunityTrashItemIfMissingAsync(Opportunity entity, CancellationToken cancellationToken)
    {
        var exists = await dbContext.GlobalTrashItems
            .AnyAsync(
                x => x.TenantId == entity.TenantId
                     && x.EntityType == CrmTrashEntityTypes.Opportunity
                     && x.EntityId == entity.Id
                     && x.Status == CrmTrashStatuses.Active,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = string.IsNullOrWhiteSpace(entity.Name) ? "Deleted opportunity" : entity.Name.Trim();
        var summary = string.IsNullOrWhiteSpace(entity.OpportunityCode) ? null : entity.OpportunityCode.Trim();

        await dbContext.GlobalTrashItems.AddAsync(
            new GlobalTrashItem
            {
                TenantId = entity.TenantId,
                EntityType = CrmTrashEntityTypes.Opportunity,
                EntityId = entity.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "opportunities",
                OriginalRoute = $"/opportunities/{entity.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId,
                DeletedByDisplayName = currentUserService.UserName ?? currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active
            },
            cancellationToken);
    }
}
