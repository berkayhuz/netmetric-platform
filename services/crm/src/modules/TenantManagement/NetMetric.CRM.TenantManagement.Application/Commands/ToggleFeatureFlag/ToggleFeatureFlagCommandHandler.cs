// <copyright file="ToggleFeatureFlagCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TenantManagement.Domain.Entities;

namespace NetMetric.CRM.TenantManagement.Application.Commands.ToggleFeatureFlag;

public sealed class ToggleFeatureFlagCommandHandler(ITenantManagementDbContext dbContext)
    : IRequestHandler<ToggleFeatureFlagCommand>
{
    public async Task Handle(ToggleFeatureFlagCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TenantFeatureFlags
            .FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.Key == request.Key, cancellationToken);

        if (entity is null)
        {
            entity = new TenantFeatureFlag(request.TenantId, request.Key, request.IsEnabled);
            await dbContext.TenantFeatureFlags.AddAsync(entity, cancellationToken);
        }

        entity.Toggle(request.IsEnabled, request.EffectiveFromUtc);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
