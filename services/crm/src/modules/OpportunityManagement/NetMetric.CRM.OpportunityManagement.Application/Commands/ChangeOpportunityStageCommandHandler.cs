// <copyright file="ChangeOpportunityStageCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class ChangeOpportunityStageCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<ChangeOpportunityStageCommand>
{
    public async Task Handle(ChangeOpportunityStageCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        var oldStage = entity.Stage;
        entity.Stage = request.NewStage;
        entity.Status = request.NewStage switch
        {
            OpportunityStageType.Won => OpportunityStatusType.Won,
            OpportunityStageType.Lost => OpportunityStatusType.Lost,
            _ => OpportunityStatusType.Open
        };
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        if (!string.IsNullOrWhiteSpace(request.RowVersion))
            entity.RowVersion = Convert.FromBase64String(request.RowVersion);

        await dbContext.OpportunityStageHistories.AddAsync(new OpportunityStageHistory
        {
            TenantId = currentUserService.TenantId,
            OpportunityId = entity.Id,
            OldStage = oldStage,
            NewStage = request.NewStage,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = currentUserService.UserId,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        }, cancellationToken);

        await outbox.EnqueueOpportunityUpdatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
