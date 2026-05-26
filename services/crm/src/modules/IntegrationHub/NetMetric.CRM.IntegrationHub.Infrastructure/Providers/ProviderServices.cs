// <copyright file="ProviderServices.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Domain.Entities;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Providers;

public sealed class DefaultIntegrationProviderCatalog : IIntegrationProviderCatalog
{
    private const string SampleVerifyValue = "sample-verify-value";
    private static readonly ProviderCapabilityDescriptor MockCapabilities = new(true, true, false, false, false, true, true);
    private static readonly ProviderCapabilityDescriptor PlaceholderCapabilities = new(true, true, true, false, true, true, true);

    private static readonly IReadOnlyCollection<ProviderCatalogItem> Providers =
    [
        new ProviderCatalogItem(
            "mock",
            "Mock Provider",
            "Local development provider for inbound and outbound omnichannel flows.",
            "mock",
            "available",
            MockCapabilities,
            [
                new ProviderFieldDescriptor("displayName", "Display Name", "Visible provider name", false, true, "Mock Provider", "Any non-empty value."),
                new ProviderFieldDescriptor("accessToken", "Access Token", "Mock token used for smoke and local tests", true, true, "smoke-access-token-...", "Minimum 12 characters."),
                new ProviderFieldDescriptor("webhookSigningSecret", "Webhook Signing Secret", "HMAC signing secret for inbound mock webhooks", true, true, "smoke-signing-secret-...", "Minimum 16 characters.")
            ],
            "Use this provider for local and CI smoke scenarios.",
            true),
        new ProviderCatalogItem(
            "whatsapp",
            "WhatsApp",
            "Configuration-ready placeholder. Real adapter is intentionally disabled in this phase.",
            "whatsapp",
            "comingSoon",
            PlaceholderCapabilities,
            [
                new ProviderFieldDescriptor("displayName", "Display Name", "Visible connection name", false, true, "WhatsApp Business", "Any non-empty value."),
                new ProviderFieldDescriptor("businessAccountId", "Business Account ID", "Meta business account identifier", false, true, "1234567890", "Numeric account id."),
                new ProviderFieldDescriptor("phoneNumberId", "Phone Number ID", "WhatsApp phone number id", false, true, "10987654321", "Numeric phone number id."),
                new ProviderFieldDescriptor("verifyToken", "Verify Token", "Webhook verify token used for challenge validation", true, true, SampleVerifyValue, "At least 8 characters."),
                new ProviderFieldDescriptor("accessToken", "Access Token", "System user token placeholder", true, true, "EAAG...", "Must start with EA and be long-lived in production."),
                new ProviderFieldDescriptor("webhookSigningSecret", "Webhook Signing Secret", "App secret used for webhook signature checks", true, true, "app-secret", "At least 16 characters.")
            ],
            "Saved configuration is draft-only until the real adapter phase is enabled.",
            false),
        new ProviderCatalogItem(
            "instagram",
            "Instagram",
            "Configuration-ready placeholder. Real adapter is intentionally disabled in this phase.",
            "instagram",
            "comingSoon",
            PlaceholderCapabilities,
            [
                new ProviderFieldDescriptor("displayName", "Display Name", "Visible connection name", false, true, "Instagram Business", "Any non-empty value."),
                new ProviderFieldDescriptor("businessAccountId", "Business Account ID", "Meta business account identifier", false, true, "1234567890", "Numeric account id."),
                new ProviderFieldDescriptor("pageId", "Page ID", "Linked Facebook page id", false, true, "9988776655", "Numeric page id."),
                new ProviderFieldDescriptor("instagramBusinessAccountId", "Instagram Business Account ID", "Instagram business account id", false, true, "1122334455", "Numeric account id."),
                new ProviderFieldDescriptor("verifyToken", "Verify Token", "Webhook verify token used for challenge validation", true, true, SampleVerifyValue, "At least 8 characters."),
                new ProviderFieldDescriptor("accessToken", "Access Token", "System user token placeholder", true, true, "EAAG...", "Must start with EA and be long-lived in production."),
                new ProviderFieldDescriptor("webhookSigningSecret", "Webhook Signing Secret", "App secret used for webhook signature checks", true, true, "app-secret", "At least 16 characters.")
            ],
            "Saved configuration is draft-only until the real adapter phase is enabled.",
            false),
        new ProviderCatalogItem(
            "genericWebhook",
            "Generic Webhook",
            "Inbound webhook placeholder for future custom provider onboarding.",
            "generic-webhook",
            "comingSoon",
            new ProviderCapabilityDescriptor(true, false, false, false, false, true, true),
            [
                new ProviderFieldDescriptor("displayName", "Display Name", "Visible connection name", false, true, "Generic Webhook", "Any non-empty value."),
                new ProviderFieldDescriptor("webhookSigningSecret", "Webhook Signing Secret", "Shared secret for inbound signatures", true, true, "shared-secret", "At least 16 characters.")
            ],
            "Use API & Webhooks for outbound subscriptions; this is for future inbound normalization.",
            false)
    ];

    private readonly IHostEnvironment _environment;
    private readonly IntegrationProviderCatalogOptions _options;

    public DefaultIntegrationProviderCatalog(IHostEnvironment environment, IOptions<IntegrationProviderCatalogOptions> options)
    {
        _environment = environment;
        _options = options.Value;
    }

    public IReadOnlyCollection<ProviderCatalogItem> List() =>
        ShouldExposeMockProvider(_environment, _options)
            ? Providers
            : Providers.Where(provider => provider.ProviderKey != "mock").ToArray();

    public ProviderCatalogItem? Find(string providerKey) =>
        List().FirstOrDefault(p => string.Equals(p.ProviderKey, providerKey?.Trim(), StringComparison.OrdinalIgnoreCase));

    internal static bool ShouldExposeMockProvider(IHostEnvironment environment, IntegrationProviderCatalogOptions options) =>
        options.MockProviderEnabled && !environment.IsProduction();
}

public sealed class DefaultProviderCredentialValidator(IIntegrationProviderCatalog catalog) : IProviderCredentialValidator
{
    public ProviderValidationResult Validate(ProviderValidationInput input)
    {
        var provider = catalog.Find(input.ProviderKey);
        if (provider is null)
        {
            return new ProviderValidationResult(false, ProviderCredentialStatuses.Error, "provider_unknown", "Provider is not registered.");
        }

        if (string.IsNullOrWhiteSpace(input.DisplayName))
        {
            return new ProviderValidationResult(false, ProviderCredentialStatuses.Draft, "display_name_required", "Display name is required.");
        }

        if (provider.ProviderKey == "mock")
        {
            if (string.IsNullOrWhiteSpace(input.AccessToken) || input.AccessToken.Trim().Length < 12)
                return new ProviderValidationResult(false, ProviderCredentialStatuses.NeedsAttention, "access_token_invalid", "Access token must be at least 12 characters.");
            if (string.IsNullOrWhiteSpace(input.WebhookSigningSecret) || input.WebhookSigningSecret.Trim().Length < 16)
                return new ProviderValidationResult(false, ProviderCredentialStatuses.NeedsAttention, "signing_secret_invalid", "Webhook signing secret must be at least 16 characters.");
            return new ProviderValidationResult(true, ProviderCredentialStatuses.Configured, "valid", "Mock provider configuration is valid.");
        }

        var config = ProviderConfigurationHelper.Parse(input.ConfigurationJson);
        var hasVerifyToken = ProviderConfigurationHelper.ReadString(config, "verifyToken").Length >= 8;
        var hasAccessToken = !string.IsNullOrWhiteSpace(input.AccessToken);
        var hasSigningSecret = !string.IsNullOrWhiteSpace(input.WebhookSigningSecret) && input.WebhookSigningSecret.Trim().Length >= 16;
        var requiresPhoneNumberId = string.Equals(provider.ProviderKey, "whatsapp", StringComparison.OrdinalIgnoreCase);
        var hasPhoneNumberId = !requiresPhoneNumberId || !string.IsNullOrWhiteSpace(ProviderConfigurationHelper.ReadString(config, "phoneNumberId"));
        return hasVerifyToken && hasAccessToken && hasSigningSecret
            && hasPhoneNumberId
            ? new ProviderValidationResult(true, ProviderCredentialStatuses.Configured, "configured", "Configuration is valid for provider readiness.")
            : new ProviderValidationResult(false, ProviderCredentialStatuses.Draft, "missing_required_fields", "Configuration saved as draft. Required fields are missing.");
    }
}

public sealed class DefaultProviderConnectionTester(
    IIntegrationProviderCatalog catalog,
    IProviderCredentialValidator validator,
    IOptions<WhatsAppProviderOptions> whatsAppOptions) : IProviderConnectionTester
{
    public ProviderConnectionTestResult Test(ProviderValidationInput input)
    {
        var provider = catalog.Find(input.ProviderKey);
        if (provider is null)
        {
            return new ProviderConnectionTestResult(false, ProviderCredentialStatuses.Error, "provider_unknown", "Provider is not registered.");
        }

        var validation = validator.Validate(input);
        if (!validation.IsValid)
        {
            return new ProviderConnectionTestResult(false, validation.Status, validation.Code, validation.Message);
        }

        if (provider.ProviderKey == "mock")
        {
            return new ProviderConnectionTestResult(true, ProviderCredentialStatuses.Connected, "mock_connection_ok", "Mock provider connection succeeded.");
        }

        if (string.Equals(provider.ProviderKey, "whatsapp", StringComparison.OrdinalIgnoreCase) &&
            whatsAppOptions.Value.OutboundEnabled)
        {
            return new ProviderConnectionTestResult(false, ProviderCredentialStatuses.Configured, "configured", "WhatsApp is configured. External connection test is disabled unless explicitly enabled.");
        }

        return new ProviderConnectionTestResult(false, ProviderCredentialStatuses.Configured, "adapter_not_active", $"{provider.DisplayName} adapter outbound is not active in this environment.");
    }
}

public sealed class ProviderAdapterRegistry(
    IEnumerable<IProviderAdapter> adapters,
    IHostEnvironment environment,
    IOptions<IntegrationProviderCatalogOptions> options) : IProviderAdapterRegistry
{
    private readonly Dictionary<string, IProviderAdapter> _map =
        adapters
            .Where(adapter => DefaultIntegrationProviderCatalog.ShouldExposeMockProvider(environment, options.Value) ||
                              !string.Equals(((IProviderWebhookVerifier)adapter).ProviderKey, "mock", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(x => ((IProviderWebhookVerifier)x).ProviderKey, StringComparer.OrdinalIgnoreCase);

    public IProviderAdapter? Resolve(string providerKey)
    {
        if (string.IsNullOrWhiteSpace(providerKey))
        {
            return null;
        }

        return _map.TryGetValue(providerKey.Trim(), out var adapter) ? adapter : null;
    }
}

public sealed class MockProviderAdapter : IProviderAdapter
{
    public string ProviderKey => "mock";

    public ProviderWebhookVerificationResult VerifyChallenge(ProviderWebhookVerificationInput input) =>
        new(false, "not_supported", "Mock provider does not support challenge verification.");

    public ProviderWebhookVerificationResult VerifyPayload(ProviderWebhookVerificationInput input) =>
        new(false, "not_supported", "Mock provider uses X-NetMetric signature verification.");

    public ProviderInboundNormalizationResult Normalize(string rawPayload) =>
        new(false, "not_supported", "Mock payload normalization is handled by legacy mock endpoint.", []);

    public ProviderOutboundSendResult Send(ProviderOutboundSendInput input) =>
        new(true, "mock_sent", "Mock outbound send is handled by omnichannel reply path.", $"mock_{Guid.NewGuid():N}");
}

public sealed class WhatsAppCloudAdapter : IProviderAdapter
{
    private readonly HttpClient _httpClient;
    private readonly WhatsAppProviderOptions _options;

    public WhatsAppCloudAdapter(HttpClient httpClient, IOptions<WhatsAppProviderOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public string ProviderKey => "whatsapp";

    public ProviderWebhookVerificationResult VerifyChallenge(ProviderWebhookVerificationInput input) =>
        MetaWebhookHelper.VerifyChallenge(input);

    public ProviderWebhookVerificationResult VerifyPayload(ProviderWebhookVerificationInput input) =>
        MetaWebhookHelper.VerifySignature(input);

    public ProviderInboundNormalizationResult Normalize(string rawPayload)
    {
        try
        {
            using var doc = JsonDocument.Parse(rawPayload);
            var messages = new List<NormalizedInboundMessage>();
            foreach (var entry in doc.RootElement.GetPropertyOrEmpty("entry"))
            {
                foreach (var change in entry.GetPropertyOrEmpty("changes"))
                {
                    var value = change.GetPropertyOrDefault("value");
                    var channelAccountId = value.GetPropertyOrDefault("metadata").GetPropertyString("phone_number_id", "wa-unknown");
                    var contacts = value.GetPropertyOrEmpty("contacts");
                    var profileName = contacts.Count > 0
                        ? contacts[0].GetPropertyOrDefault("profile").GetPropertyString("name", "Unknown")
                        : "Unknown";
                    foreach (var message in value.GetPropertyOrEmpty("messages"))
                    {
                        var messageId = message.GetPropertyString("id", string.Empty);
                        if (string.IsNullOrWhiteSpace(messageId))
                        {
                            continue;
                        }

                        var sender = message.GetPropertyString("from", "unknown");
                        var text = message.GetPropertyOrDefault("text").GetPropertyString("body", string.Empty);
                        var sentAt = MetaWebhookHelper.ParseUnixTimestamp(message.GetPropertyString("timestamp", null));
                        messages.Add(new NormalizedInboundMessage(
                            ProviderKey,
                            messageId,
                            messageId,
                            channelAccountId,
                            sender,
                            sender,
                            profileName,
                            text,
                            sentAt));
                    }
                }
            }

            return messages.Count == 0
                ? new ProviderInboundNormalizationResult(false, "no_messages", "No supported WhatsApp messages were found in payload.", [])
                : new ProviderInboundNormalizationResult(true, "normalized", "Payload normalized.", messages);
        }
        catch (JsonException)
        {
            return new ProviderInboundNormalizationResult(false, "invalid_payload", "Payload is not valid JSON.", []);
        }
    }

    public ProviderOutboundSendResult Send(ProviderOutboundSendInput input) =>
        SendInternalAsync(input, CancellationToken.None).GetAwaiter().GetResult();

    public async Task<ProviderOutboundSendResult> SendAsync(ProviderOutboundSendInput input, CancellationToken cancellationToken = default) =>
        await SendInternalAsync(input, cancellationToken);

    private async Task<ProviderOutboundSendResult> SendInternalAsync(ProviderOutboundSendInput input, CancellationToken cancellationToken)
    {
        if (!_options.OutboundEnabled)
        {
            return new(false, "adapter_not_active", "WhatsApp outbound adapter is disabled by feature flag.");
        }

        var config = ProviderConfigurationHelper.Parse(input.ConfigurationJson);
        var phoneNumberId = ProviderConfigurationHelper.ReadString(config, "phoneNumberId");
        var graphApiVersion = ProviderConfigurationHelper.ReadString(config, "graphApiVersion");
        var graphApiBaseUrl = ProviderConfigurationHelper.ReadString(config, "graphApiBaseUrl");
        var accessToken = input.Secrets.AccessToken?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(accessToken) ||
            string.IsNullOrWhiteSpace(phoneNumberId) ||
            string.IsNullOrWhiteSpace(input.RecipientExternalId) ||
            string.IsNullOrWhiteSpace(input.Text))
        {
            return new(false, "missing_required_fields", "WhatsApp outbound configuration is incomplete.");
        }

        var baseUrl = string.IsNullOrWhiteSpace(graphApiBaseUrl) ? _options.GraphApiBaseUrl : graphApiBaseUrl.Trim();
        var apiVersion = string.IsNullOrWhiteSpace(graphApiVersion) ? _options.GraphApiVersion : graphApiVersion.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiVersion))
        {
            return new(false, "missing_required_fields", "Graph API base URL or version is missing.");
        }

        var endpoint = $"{baseUrl.TrimEnd('/')}/{apiVersion.Trim('/')}/{phoneNumberId}/messages";
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = JsonContent.Create(new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = input.RecipientExternalId,
            type = "text",
            text = new { body = input.Text }
        });

        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, _options.TimeoutSeconds)));
            using var response = await _httpClient.SendAsync(request, cts.Token);
            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                var externalMessageId = TryReadWhatsAppMessageId(body);
                return new(true, "sent", "WhatsApp outbound message sent.", externalMessageId);
            }

            if ((int)response.StatusCode >= 500)
            {
                return new(false, "provider_transient_failure", $"WhatsApp Graph API transient failure ({(int)response.StatusCode}).");
            }

            return new(false, "provider_permanent_failure", $"WhatsApp Graph API rejected request ({(int)response.StatusCode}).");
        }
        catch (OperationCanceledException)
        {
            return new(false, "provider_transient_failure", "WhatsApp Graph API request timed out.");
        }
        catch (HttpRequestException)
        {
            return new(false, "provider_transient_failure", "WhatsApp Graph API request failed.");
        }
    }

    private static string? TryReadWhatsAppMessageId(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var messages = doc.RootElement.GetPropertyOrEmpty("messages");
            if (messages.Count == 0)
            {
                return null;
            }

            return messages[0].GetPropertyString("id", null);
        }
        catch
        {
            return null;
        }
    }
}

public sealed class InstagramMessagingAdapter : IProviderAdapter
{
    public string ProviderKey => "instagram";

    public ProviderWebhookVerificationResult VerifyChallenge(ProviderWebhookVerificationInput input) =>
        MetaWebhookHelper.VerifyChallenge(input);

    public ProviderWebhookVerificationResult VerifyPayload(ProviderWebhookVerificationInput input) =>
        MetaWebhookHelper.VerifySignature(input);

    public ProviderInboundNormalizationResult Normalize(string rawPayload)
    {
        using var doc = JsonDocument.Parse(rawPayload);
        var messages = new List<NormalizedInboundMessage>();
        foreach (var entry in doc.RootElement.GetPropertyOrEmpty("entry"))
        {
            foreach (var item in entry.GetPropertyOrEmpty("messaging"))
            {
                var message = item.GetPropertyOrDefault("message");
                var messageId = message.GetPropertyString("mid", string.Empty);
                if (string.IsNullOrWhiteSpace(messageId))
                {
                    continue;
                }

                var senderId = item.GetPropertyOrDefault("sender").GetPropertyString("id", "unknown");
                var recipientId = item.GetPropertyOrDefault("recipient").GetPropertyString("id", "ig-unknown");
                var text = message.GetPropertyString("text", string.Empty);
                var sentAt = MetaWebhookHelper.ParseUnixMilliseconds(item.GetPropertyOrDefault("timestamp").GetInt64OrDefault(0));

                messages.Add(new NormalizedInboundMessage(
                    ProviderKey,
                    messageId,
                    messageId,
                    recipientId,
                    senderId,
                    senderId,
                    senderId,
                    text,
                    sentAt));
            }
        }

        return messages.Count == 0
            ? new ProviderInboundNormalizationResult(false, "no_messages", "No supported Instagram messages were found in payload.", [])
            : new ProviderInboundNormalizationResult(true, "normalized", "Payload normalized.", messages);
    }

    public ProviderOutboundSendResult Send(ProviderOutboundSendInput input) =>
        new(false, "adapter_not_active", "Instagram outbound adapter is not active in this phase.");
}

public sealed class WhatsAppProviderOptions
{
    public const string SectionName = "Crm:IntegrationProviders:WhatsApp";
    public const string HttpClientName = "integrationhub.whatsapp";

    public bool OutboundEnabled { get; set; }
    public bool ExternalValidationEnabled { get; set; }
    public string GraphApiBaseUrl { get; set; } = "https://graph.facebook.com";
    public string GraphApiVersion { get; set; } = "v23.0";
    public int TimeoutSeconds { get; set; } = 10;
}

public sealed class IntegrationProviderCatalogOptions
{
    public const string SectionName = "Crm:IntegrationProviders";

    public bool MockProviderEnabled { get; set; } = true;
}

public sealed class IntegrationProviderCatalogOptionsValidation(IHostEnvironment environment) : IValidateOptions<IntegrationProviderCatalogOptions>
{
    public ValidateOptionsResult Validate(string? name, IntegrationProviderCatalogOptions options)
    {
        if (environment.IsProduction() && options.MockProviderEnabled)
        {
            return ValidateOptionsResult.Fail("Crm:IntegrationProviders:MockProviderEnabled must be false in Production.");
        }

        return ValidateOptionsResult.Success;
    }
}

internal static class ProviderConfigurationHelper
{
    public static JsonElement? Parse(string? configurationJson)
    {
        if (string.IsNullOrWhiteSpace(configurationJson))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(configurationJson);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    public static string ReadString(JsonElement? config, string name)
    {
        if (config is null || config.Value.ValueKind != JsonValueKind.Object)
        {
            return string.Empty;
        }

        return config.Value.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()?.Trim() ?? string.Empty
            : string.Empty;
    }
}

internal static class MetaWebhookHelper
{
    public static ProviderWebhookVerificationResult VerifyChallenge(ProviderWebhookVerificationInput input)
    {
        var mode = input.VerifyMode?.Trim();
        var verifyToken = input.VerifyToken?.Trim();
        var expected = input.Secrets.VerifyToken?.Trim();
        if (!string.Equals(mode, "subscribe", StringComparison.OrdinalIgnoreCase))
        {
            return new(false, "invalid_verify_mode", "Webhook verification mode is invalid.");
        }

        if (string.IsNullOrWhiteSpace(expected))
        {
            return new(false, "missing_required_fields", "Provider verify token is not configured.");
        }

        if (!string.Equals(verifyToken, expected, StringComparison.Ordinal))
        {
            return new(false, "invalid_verify_token", "Verify token is invalid.");
        }

        return new(true, "configured", "Verification challenge accepted.", input.Challenge ?? string.Empty);
    }

    public static ProviderWebhookVerificationResult VerifySignature(ProviderWebhookVerificationInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Secrets.WebhookSigningSecret))
        {
            return new(false, "missing_required_fields", "Webhook signing secret is missing.");
        }

        var header = input.SignatureHeader?.Trim();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase))
        {
            return new(false, "invalid_signature", "Signature header is missing or malformed.");
        }

        var provided = header.Substring("sha256=".Length);
        var computed = ComputeHmacHex(input.Secrets.WebhookSigningSecret, input.RawPayload);
        var providedBytes = Encoding.UTF8.GetBytes(provided);
        var computedBytes = Encoding.UTF8.GetBytes(computed);
        var valid = CryptographicOperations.FixedTimeEquals(providedBytes, computedBytes);
        return valid
            ? new ProviderWebhookVerificationResult(true, "configured", "Signature verified.")
            : new ProviderWebhookVerificationResult(false, "invalid_signature", "Signature verification failed.");
    }

    public static DateTime ParseUnixTimestamp(string? value)
    {
        if (long.TryParse(value, out var seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }

        return DateTime.UtcNow;
    }

    public static DateTime ParseUnixMilliseconds(long value)
    {
        return value > 0 ? DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime : DateTime.UtcNow;
    }

    private static string ComputeHmacHex(string secret, string payload)
    {
        var key = Encoding.UTF8.GetBytes(secret.Trim());
        var bytes = Encoding.UTF8.GetBytes(payload);
        using var hmac = new HMACSHA256(key);
        return Convert.ToHexString(hmac.ComputeHash(bytes)).ToLowerInvariant();
    }
}

internal static class JsonElementExtensions
{
    public static IReadOnlyList<JsonElement> GetPropertyOrEmpty(this JsonElement element, string name)
    {
        var prop = element.GetPropertyOrDefault(name);
        if (prop.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        return prop.EnumerateArray().Select(x => x.Clone()).ToArray();
    }

    public static JsonElement GetPropertyOrDefault(this JsonElement element, string name)
    {
        return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value)
            ? value
            : default;
    }

    public static string GetPropertyString(this JsonElement element, string name, string? fallback)
    {
        var prop = element.GetPropertyOrDefault(name);
        if (prop.ValueKind == JsonValueKind.String)
        {
            return prop.GetString() ?? fallback ?? string.Empty;
        }

        if (prop.ValueKind == JsonValueKind.Number)
        {
            return prop.ToString();
        }

        return fallback ?? string.Empty;
    }

    public static long GetInt64OrDefault(this JsonElement element, long fallback)
    {
        if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var value))
        {
            return value;
        }

        return fallback;
    }
}
