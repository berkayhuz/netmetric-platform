// <copyright file="CreateLostReasonCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class CreateLostReasonCommandHandler(IPipelineManagementDbContext dbContext, ICurrentUserService currentUserService)
    : IRequestHandler<CreateLostReasonCommand, LostReasonDto>
{
    public async Task<LostReasonDto> Handle(CreateLostReasonCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        if (request.IsDefault)
        {
            var existingDefaults = await dbContext.LostReasons
                .Where(x => x.IsDefault && x.TenantId == tenantId)
                .ToListAsync(cancellationToken);
            foreach (var item in existingDefaults)
                item.IsDefault = false;
        }

        var entity = new LostReason
        {
            TenantId = tenantId,
            Name = request.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await dbContext.LostReasons.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
