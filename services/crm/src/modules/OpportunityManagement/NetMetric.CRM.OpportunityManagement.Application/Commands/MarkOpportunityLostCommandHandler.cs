// <copyright file="MarkOpportunityLostCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class MarkOpportunityLostCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<MarkOpportunityLostCommand>
{
    public async Task Handle(MarkOpportunityLostCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
            opportunity.RowVersion = Convert.FromBase64String(request.RowVersion);

        opportunity.Stage = OpportunityStageType.Lost;
        opportunity.Status = OpportunityStatusType.Lost;
        opportunity.LostReasonId = request.LostReasonId;
        opportunity.LostNote = string.IsNullOrWhiteSpace(request.LostNote) ? null : request.LostNote.Trim();
        opportunity.UpdatedAt = DateTime.UtcNow;
        opportunity.UpdatedBy = currentUserService.UserName;
        await outbox.EnqueueOpportunityUpdatedAsync(opportunity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
