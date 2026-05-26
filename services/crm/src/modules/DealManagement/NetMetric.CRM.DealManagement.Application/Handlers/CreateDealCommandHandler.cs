// <copyright file="CreateDealCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.DealManagement.Application.Common;
using NetMetric.CRM.DealManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class CreateDealCommandHandler(
    IDealManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IDealManagementOutbox outbox) : IRequestHandler<CreateDealCommand, DealDetailDto>
{
    public async Task<DealDetailDto> Handle(CreateDealCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        EnsureSupportedReferences(request.OpportunityId, request.CompanyId);

        var entity = new Deal
        {
            TenantId = currentUserService.TenantId,
            DealCode = request.DealCode.Trim(),
            Name = request.Name.Trim(),
            TotalAmount = request.TotalAmount,
            ClosedDate = request.ClosedDate,
            OpportunityId = request.OpportunityId,
            CompanyId = request.CompanyId,
            OwnerUserId = request.OwnerUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };
        entity.SetNotes(request.Notes);

        await dbContext.Deals.AddAsync(entity, cancellationToken);
        await outbox.EnqueueDealCreatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await DealHandlerHelpers.AppendHistoryAsync(dbContext, currentUserService, entity, entity.TotalAmount > 0m ? "Won" : "Pending", entity.TotalAmount > 0m ? "Won" : "Open", DateTime.UtcNow, null, "Deal created.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.ToDetailDto(null, await DealHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken));
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
