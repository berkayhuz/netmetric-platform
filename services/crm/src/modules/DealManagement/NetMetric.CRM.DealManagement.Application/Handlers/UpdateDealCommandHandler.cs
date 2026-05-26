// <copyright file="UpdateDealCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.DealManagement.Application.Common;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class UpdateDealCommandHandler(
    IDealManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IDealManagementOutbox outbox) : IRequestHandler<UpdateDealCommand, DealDetailDto>
{
    public async Task<DealDetailDto> Handle(UpdateDealCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await DealHandlerHelpers.RequireDealAsync(dbContext, request.DealId, cancellationToken);
        DealHandlerHelpers.ApplyRowVersion(entity, request.RowVersion);
        EnsureSupportedReferences(request.OpportunityId, request.CompanyId);

        entity.DealCode = request.DealCode.Trim();
        entity.Name = request.Name.Trim();
        entity.TotalAmount = request.TotalAmount;
        entity.ClosedDate = request.ClosedDate;
        entity.OpportunityId = request.OpportunityId;
        entity.CompanyId = request.CompanyId;
        entity.OwnerUserId = request.OwnerUserId;
        entity.SetNotes(request.Notes);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        await outbox.EnqueueDealUpdatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDetailDto(await DealHandlerHelpers.LoadReviewAsync(dbContext, entity.Id, cancellationToken), await DealHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken));
    }

    private static void EnsureSupportedReferences(Guid? opportunityId, Guid? companyId)
    {
        if (opportunityId.HasValue)
        {
            throw new ValidationAppException("OpportunityId cannot be linked directly in the DealManagement write model.");
        }

        if (companyId.HasValue)
        {
            throw new ValidationAppException("CompanyId cannot be linked directly in the DealManagement write model.");
        }
    }
}
