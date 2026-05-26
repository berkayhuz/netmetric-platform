// <copyright file="GetConnectorHealthQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.DTOs;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Queries.GetConnectorHealth;

public sealed class GetConnectorHealthQueryHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext,
    IIntegrationConnectorRegistry connectorRegistry) : IRequestHandler<GetConnectorHealthQuery, IReadOnlyCollection<IntegrationConnectorHealthDto>>
{
    public async Task<IReadOnlyCollection<IntegrationConnectorHealthDto>> Handle(GetConnectorHealthQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var now = DateTime.UtcNow;
        var connections = await dbContext.IntegrationConnections
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.ProviderKey)
            .ToListAsync(cancellationToken);

        foreach (var connection in connections)
        {
            if (!connection.IsEnabled)
            {
                connection.RecordHealth(IntegrationConnectorHealthStates.Disabled, "Connection is disabled.", now);
                continue;
            }

            var connector = connectorRegistry.Resolve(connection.ProviderKey);
            if (connector is null)
            {
                connection.RecordHealth(IntegrationConnectorHealthStates.NotConfigured, "No connector adapter is registered for this provider.", now);
                continue;
            }

            var health = await connector.CheckHealthAsync(connection, cancellationToken);
            connection.RecordHealth(health.Status, health.Message, health.CheckedAtUtc);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return connections
            .Select(x => new IntegrationConnectorHealthDto(
                x.Id,
                x.ProviderKey,
                x.DisplayName,
                x.Category,
                x.IsEnabled,
                x.HealthStatus,
                x.HealthMessage,
                x.LastHealthCheckAtUtc,
                x.SecretVersion,
                x.SecretRotatedAtUtc))
            .ToList();
    }
}
