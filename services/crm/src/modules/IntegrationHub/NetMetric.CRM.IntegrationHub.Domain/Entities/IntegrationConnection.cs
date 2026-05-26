// <copyright file="IntegrationConnection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class IntegrationConnection : EntityBase
{
    public string ProviderKey { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string Category { get; private set; } = null!;
    public string SettingsJson { get; private set; } = "{}";
    public bool IsEnabled { get; private set; }
    public string HealthStatus { get; private set; } = IntegrationConnectorHealthStates.Unknown;
    public string? HealthMessage { get; private set; }
    public DateTime? LastHealthCheckAtUtc { get; private set; }
    public string? DeltaSyncToken { get; private set; }
    public int SecretVersion { get; private set; } = 1;
    public DateTime? SecretRotatedAtUtc { get; private set; }

    private IntegrationConnection() { }

    public IntegrationConnection(Guid tenantId, string providerKey, string displayName, string category, string settingsJson)
    {
        TenantId = tenantId;
        ProviderKey = providerKey.Trim();
        DisplayName = displayName.Trim();
        Category = category.Trim();
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson.Trim();
        IsEnabled = true;
    }

    public void Reconfigure(string displayName, string settingsJson, bool isEnabled)
    {
        DisplayName = displayName.Trim();
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : settingsJson.Trim();
        IsEnabled = isEnabled;
    }

    public void RecordHealth(string status, string? message, DateTime checkedAtUtc)
    {
        HealthStatus = Guard.AgainstNullOrWhiteSpace(status).Trim();
        HealthMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        LastHealthCheckAtUtc = checkedAtUtc;
    }

    public void UpdateDeltaSyncToken(string? deltaSyncToken)
    {
        DeltaSyncToken = string.IsNullOrWhiteSpace(deltaSyncToken) ? null : deltaSyncToken.Trim();
    }

    public void MarkSecretRotated(DateTime rotatedAtUtc)
    {
        SecretVersion += 1;
        SecretRotatedAtUtc = rotatedAtUtc;
    }
}

public static class IntegrationConnectorHealthStates
{
    public const string Unknown = "unknown";
    public const string Healthy = "healthy";
    public const string Degraded = "degraded";
    public const string Unhealthy = "unhealthy";
    public const string NotConfigured = "not_configured";
    public const string Disabled = "disabled";
}
