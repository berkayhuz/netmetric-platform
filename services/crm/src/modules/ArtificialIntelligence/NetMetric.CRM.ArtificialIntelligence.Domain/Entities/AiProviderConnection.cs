// <copyright file="AiProviderConnection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ArtificialIntelligence.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

public sealed class AiProviderConnection : AuditableEntity
{
    private AiProviderConnection()
    {
    }

    public AiProviderConnection(string name, AiProviderType provider, string modelName, string endpoint, string secretReference, IReadOnlyCollection<AiCapabilityType> capabilities)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Provider name is required.", nameof(name)) : name.Trim();
        Provider = provider;
        ModelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name is required.", nameof(modelName)) : modelName.Trim();
        Endpoint = string.IsNullOrWhiteSpace(endpoint) ? throw new ArgumentException("Endpoint is required.", nameof(endpoint)) : endpoint.Trim();
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? throw new ArgumentException("Secret reference is required.", nameof(secretReference)) : secretReference.Trim();
        Capabilities = string.Join(',', capabilities.Distinct().OrderBy(x => x));
    }

    public string Name { get; private set; } = null!;
    public AiProviderType Provider { get; private set; }
    public string ModelName { get; private set; } = null!;
    public string Endpoint { get; private set; } = null!;
    public string SecretReference { get; private set; } = null!;
    public string Capabilities { get; private set; } = string.Empty;

    public void Update(string name, string modelName, string endpoint, string secretReference, IReadOnlyCollection<AiCapabilityType> capabilities, bool isActive)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Provider name is required.", nameof(name)) : name.Trim();
        ModelName = string.IsNullOrWhiteSpace(modelName) ? throw new ArgumentException("Model name is required.", nameof(modelName)) : modelName.Trim();
        Endpoint = string.IsNullOrWhiteSpace(endpoint) ? throw new ArgumentException("Endpoint is required.", nameof(endpoint)) : endpoint.Trim();
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? throw new ArgumentException("Secret reference is required.", nameof(secretReference)) : secretReference.Trim();
        Capabilities = string.Join(',', capabilities.Distinct().OrderBy(x => x));
        SetActive(isActive);
    }
}
