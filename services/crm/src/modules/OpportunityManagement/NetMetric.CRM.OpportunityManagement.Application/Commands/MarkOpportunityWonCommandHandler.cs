// <copyright file="MarkOpportunityWonCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class MarkOpportunityWonCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IDealManagementDbContext dealManagementDbContext,
    IDealManagementOutbox dealManagementOutbox,
    IOpportunityManagementOutbox opportunityOutbox) : IRequestHandler<MarkOpportunityWonCommand, Guid?>
{
    public async Task<Guid?> Handle(MarkOpportunityWonCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
            opportunity.RowVersion = Convert.FromBase64String(request.RowVersion);

        opportunity.Stage = OpportunityStageType.Won;
        opportunity.Status = OpportunityStatusType.Won;
        opportunity.UpdatedAt = DateTime.UtcNow;
        opportunity.UpdatedBy = currentUserService.UserName;

        await opportunityOutbox.EnqueueOpportunityUpdatedAsync(opportunity, cancellationToken);
        var deal = new Deal
        {
            TenantId = currentUserService.TenantId,
            DealCode = $"DEAL-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Name = string.IsNullOrWhiteSpace(request.DealName) ? opportunity.Name : request.DealName.Trim(),
            TotalAmount = opportunity.ExpectedRevenue ?? opportunity.EstimatedAmount,
            ClosedDate = request.ClosedDate,
            OpportunityId = null,
            CustomerId = null,
            OwnerUserId = opportunity.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await dbContext.SaveChangesAsync(cancellationToken);

        await dealManagementDbContext.Deals.AddAsync(deal, cancellationToken);
        await dealManagementDbContext.SaveChangesAsync(cancellationToken);
        await dealManagementOutbox.EnqueueDealCreatedAndPersistAsync(deal, cancellationToken);
        return deal.Id;
    }
}
