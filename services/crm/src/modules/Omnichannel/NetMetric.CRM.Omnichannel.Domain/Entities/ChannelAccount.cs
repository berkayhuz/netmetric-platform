// <copyright file="ChannelAccount.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Omnichannel.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.Omnichannel.Domain.Entities;

public sealed class ChannelAccount : AuditableEntity
{
    private ChannelAccount()
    {
    }

    public ChannelAccount(string name, ChannelType channelType, string externalAccountId, string secretReference, string routingKey)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Account name is required.", nameof(name)) : name.Trim();
        ChannelType = channelType;
        ExternalAccountId = string.IsNullOrWhiteSpace(externalAccountId) ? throw new ArgumentException("External account id is required.", nameof(externalAccountId)) : externalAccountId.Trim();
        SecretReference = string.IsNullOrWhiteSpace(secretReference) ? throw new ArgumentException("Secret reference is required.", nameof(secretReference)) : secretReference.Trim();
        RoutingKey = string.IsNullOrWhiteSpace(routingKey) ? throw new ArgumentException("Routing key is required.", nameof(routingKey)) : routingKey.Trim();
        ProviderKey = "mock";
        IsActive = true;
    }

    public string Name { get; private set; } = null!;
    public ChannelType ChannelType { get; private set; }
    public string ExternalAccountId { get; private set; } = null!;
    public string SecretReference { get; private set; } = null!;
    public string RoutingKey { get; private set; } = null!;
    public string ProviderKey { get; private set; } = null!;
    public Guid? ProviderCredentialId { get; private set; }
    public new bool IsActive { get; private set; }

    public void SetProvider(string providerKey)
    {
        ProviderKey = string.IsNullOrWhiteSpace(providerKey)
            ? throw new ArgumentException("Provider key is required.", nameof(providerKey))
            : providerKey.Trim().ToLowerInvariant();
    }

    public void LinkProviderCredential(Guid providerCredentialId)
    {
        ProviderCredentialId = providerCredentialId;
    }
}
