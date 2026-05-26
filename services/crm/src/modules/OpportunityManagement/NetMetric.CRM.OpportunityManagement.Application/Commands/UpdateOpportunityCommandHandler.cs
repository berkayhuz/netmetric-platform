// <copyright file="UpdateOpportunityCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class UpdateOpportunityCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityManagementOutbox outbox) : IRequestHandler<UpdateOpportunityCommand, OpportunityDetailDto>
{
    public async Task<OpportunityDetailDto> Handle(UpdateOpportunityCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");
        EnsureSupportedReferences(request.LeadId, request.CustomerId);

        entity.OpportunityCode = request.OpportunityCode.Trim();
        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.EstimatedAmount = request.EstimatedAmount;
        entity.ExpectedRevenue = request.ExpectedRevenue;
        entity.Probability = request.Probability;
        entity.EstimatedCloseDate = request.EstimatedCloseDate;
        entity.Stage = request.Stage;
        entity.Status = request.Status;
        entity.Priority = request.Priority;
        entity.LeadId = request.LeadId;
        entity.CustomerId = request.CustomerId;
        entity.OwnerUserId = request.OwnerUserId;
        entity.SetNotes(request.Notes);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        entity.RowVersion = Convert.FromBase64String(request.RowVersion);

        await outbox.EnqueueOpportunityUpdatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var products = await dbContext.OpportunityProducts.Where(x => x.OpportunityId == entity.Id).ToListAsync(cancellationToken);
        var contacts = await dbContext.OpportunityContacts.Where(x => x.OpportunityId == entity.Id).ToListAsync(cancellationToken);
        var quotes = await dbContext.Quotes.Include(x => x.Items).Where(x => x.OpportunityId == entity.Id).ToListAsync(cancellationToken);
        var stageHistory = await dbContext.OpportunityStageHistories.Where(x => x.OpportunityId == entity.Id).OrderByDescending(x => x.ChangedAt).ToListAsync(cancellationToken);
        return entity.ToDetailDto(products, contacts, quotes, stageHistory);
    }

    private static void EnsureSupportedReferences(Guid? leadId, Guid? customerId)
    {
        if (leadId.HasValue)
        {
            throw new ValidationAppException("LeadId cannot be linked directly in the OpportunityManagement write model.");
        }

        if (customerId.HasValue)
        {
            throw new ValidationAppException("CustomerId cannot be linked directly in the OpportunityManagement write model.");
        }
    }
}
