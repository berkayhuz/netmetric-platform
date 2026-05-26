// <copyright file="DeleteLostReasonCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class DeleteLostReasonCommandHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<DeleteLostReasonCommand>
{
    public async Task Handle(DeleteLostReasonCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var entity = await dbContext.LostReasons.FirstOrDefaultAsync(
                x => x.Id == request.LostReasonId && x.TenantId == tenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Lost reason not found.");

        dbContext.LostReasons.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
