// <copyright file="SupportInboxConnection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.SupportInboxIntegration.Domain.Enums;
using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.SupportInboxIntegration.Domain.Entities;

public sealed class SupportInboxConnection : AuditableEntity
{
    private SupportInboxConnection() { }

    public SupportInboxConnection(string name, SupportInboxProviderType provider, string emailAddress, string host, int port, string username, string secretReference, bool useSsl)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Provider = provider;
        EmailAddress = Guard.AgainstNullOrWhiteSpace(emailAddress);
        Host = Guard.AgainstNullOrWhiteSpace(host);
        Port = port;
        Username = Guard.AgainstNullOrWhiteSpace(username);
        SecretReference = Guard.AgainstNullOrWhiteSpace(secretReference);
        UseSsl = useSsl;
    }

    public string Name { get; private set; } = null!;
    public SupportInboxProviderType Provider { get; private set; }
    public string EmailAddress { get; private set; } = null!;
    public string Host { get; private set; } = null!;
    public int Port { get; private set; }
    public string Username { get; private set; } = null!;
    public string SecretReference { get; private set; } = null!;
    public bool UseSsl { get; private set; }
    public void Update(string name, string host, int port, string username, string secretReference, bool useSsl, bool isActive)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name);
        Host = Guard.AgainstNullOrWhiteSpace(host);
        Port = port;
        Username = Guard.AgainstNullOrWhiteSpace(username);
        SecretReference = Guard.AgainstNullOrWhiteSpace(secretReference);
        UseSsl = useSsl;
        SetActive(isActive);
    }
}
