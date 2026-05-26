// <copyright file="AssignDealOwnerCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class AssignDealOwnerCommandHandler(IDealManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<AssignDealOwnerCommand>
{
    public async Task Handle(AssignDealOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await DealHandlerHelpers.RequireDealAsync(dbContext, request.DealId, cancellationToken);
        entity.OwnerUserId = request.OwnerUserId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
