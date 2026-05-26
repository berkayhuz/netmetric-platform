// <copyright file="UpdateLostReasonCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class UpdateLostReasonCommandHandler(IPipelineManagementDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateLostReasonCommand, LostReasonDto>
{
    public async Task<LostReasonDto> Handle(UpdateLostReasonCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var entity = await dbContext.LostReasons.FirstOrDefaultAsync(
                x => x.Id == request.LostReasonId && x.TenantId == tenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Lost reason not found.");

        if (!string.IsNullOrWhiteSpace(request.RowVersion))
            entity.RowVersion = Convert.FromBase64String(request.RowVersion);

        if (request.IsDefault)
        {
            var existingDefaults = await dbContext.LostReasons
                .Where(x => x.IsDefault && x.Id != request.LostReasonId && x.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        entity.Name = request.Name.Trim();
        entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        entity.IsDefault = request.IsDefault;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
