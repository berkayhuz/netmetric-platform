// <copyright file="ProviderCredential.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;
using NetMetric.Guards;

namespace NetMetric.CRM.IntegrationHub.Domain.Entities;

public sealed class ProviderCredential : EntityBase
{
    private ProviderCredential() { }

    public string ProviderKey { get; private set; } = null!;
    public string DisplayName { get; private set; } = null!;
    public string EndpointKey { get; private set; } = null!;
    public string AccessToken { get; private set; } = null!;
    public string WebhookSigningSecret { get; private set; } = null!;
    public string? ConfigurationJson { get; private set; }
    public string Status { get; private set; } = ProviderCredentialStatuses.Draft;
    public string? LastValidationStatus { get; private set; }
    public string? LastValidationCode { get; private set; }
    public string? LastValidationMessage { get; private set; }
    public DateTime? LastValidatedAtUtc { get; private set; }
    public bool IsEnabled { get; private set; }

    public ProviderCredential(Guid tenantId, string providerKey, string displayName, string accessToken, string webhookSigningSecret)
    {
        TenantId = tenantId;
        ProviderKey = Guard.AgainstNullOrWhiteSpace(providerKey).Trim().ToLowerInvariant();
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName).Trim();
        AccessToken = accessToken?.Trim() ?? string.Empty;
        WebhookSigningSecret = webhookSigningSecret?.Trim() ?? string.Empty;
        EndpointKey = $"ep_{Guid.NewGuid():N}";
        IsEnabled = true;
        Status = ProviderCredentialStatuses.Draft;
    }

    public void Reconfigure(string displayName, string accessToken, string webhookSigningSecret, bool isEnabled, string? configurationJson = null)
    {
        DisplayName = Guard.AgainstNullOrWhiteSpace(displayName).Trim();
        AccessToken = accessToken?.Trim() ?? string.Empty;
        WebhookSigningSecret = webhookSigningSecret?.Trim() ?? string.Empty;
        IsEnabled = isEnabled;
        ConfigurationJson = configurationJson;
    }

    public void SetStatus(string status)
    {
        Status = Guard.AgainstNullOrWhiteSpace(status).Trim();
    }

    public void RecordValidation(string status, string? code, string? message, DateTime validatedAtUtc)
    {
        LastValidationStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        LastValidationCode = string.IsNullOrWhiteSpace(code) ? null : code.Trim();
        LastValidationMessage = string.IsNullOrWhiteSpace(message) ? null : message.Trim();
        LastValidatedAtUtc = validatedAtUtc;
    }
}

public static class ProviderCredentialStatuses
{
    public const string Draft = "draft";
    public const string Configured = "configured";
    public const string Connected = "connected";
    public const string NeedsAttention = "needsAttention";
    public const string Disabled = "disabled";
    public const string Error = "error";
}
