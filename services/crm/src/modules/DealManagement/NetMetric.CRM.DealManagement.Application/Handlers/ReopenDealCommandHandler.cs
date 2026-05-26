// <copyright file="ReopenDealCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class ReopenDealCommandHandler(IDealManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<ReopenDealCommand>
{
    public async Task Handle(ReopenDealCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await DealHandlerHelpers.RequireDealAsync(dbContext, request.DealId, cancellationToken);
        DealHandlerHelpers.ApplyRowVersion(entity, request.RowVersion);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await DealHandlerHelpers.AppendHistoryAsync(dbContext, currentUserService, entity, "Pending", "Open", DateTime.UtcNow, null, request.Note, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
