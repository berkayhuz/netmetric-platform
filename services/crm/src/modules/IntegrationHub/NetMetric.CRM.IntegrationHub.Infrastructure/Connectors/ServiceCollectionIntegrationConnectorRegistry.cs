// <copyright file="ServiceCollectionIntegrationConnectorRegistry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Connectors;

public sealed class ServiceCollectionIntegrationConnectorRegistry(IEnumerable<IIntegrationConnector> connectors) : IIntegrationConnectorRegistry
{
    private readonly IReadOnlyDictionary<string, IIntegrationConnector> _connectors = connectors
        .GroupBy(x => x.ProviderKey, StringComparer.OrdinalIgnoreCase)
        .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

    public IIntegrationConnector? Resolve(string providerKey)
        => _connectors.TryGetValue(providerKey.Trim(), out var connector) ? connector : null;
}
