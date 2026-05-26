// <copyright file="AddOpportunityContactCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Commands;

public sealed class AddOpportunityContactCommandHandler(IOpportunityManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<AddOpportunityContactCommand, OpportunityContactDto>
{
    public async Task<OpportunityContactDto> Handle(AddOpportunityContactCommand request, CancellationToken cancellationToken)
    {
        var exists = await dbContext.Opportunities.AnyAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken);
        if (!exists) throw new NotFoundAppException("Opportunity not found.");

        if (request.IsPrimary)
        {
            var primaryItems = await dbContext.OpportunityContacts.Where(x => x.OpportunityId == request.OpportunityId && x.IsPrimary).ToListAsync(cancellationToken);
            foreach (var item in primaryItems) item.IsPrimary = false;
        }

        var entity = new OpportunityContact
        {
            TenantId = currentUserService.TenantId,
            OpportunityId = request.OpportunityId,
            ContactId = request.ContactId,
            IsDecisionMaker = request.IsDecisionMaker,
            IsPrimary = request.IsPrimary,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await dbContext.OpportunityContacts.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
