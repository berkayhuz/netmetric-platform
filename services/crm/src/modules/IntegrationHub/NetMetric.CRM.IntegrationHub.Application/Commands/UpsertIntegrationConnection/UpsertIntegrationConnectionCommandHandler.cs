// <copyright file="UpsertIntegrationConnectionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.UpsertIntegrationConnection;

public sealed class UpsertIntegrationConnectionCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<UpsertIntegrationConnectionCommand, Guid>
{
    public async Task<Guid> Handle(UpsertIntegrationConnectionCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);

        var entity = await dbContext.IntegrationConnections
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ProviderKey == request.ProviderKey, cancellationToken);

        if (entity is null)
        {
            entity = new IntegrationConnection(tenantId, request.ProviderKey, request.DisplayName, request.Category, request.SettingsJson);
            await dbContext.IntegrationConnections.AddAsync(entity, cancellationToken);
        }

        entity.Reconfigure(request.DisplayName, request.SettingsJson, request.IsEnabled);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
