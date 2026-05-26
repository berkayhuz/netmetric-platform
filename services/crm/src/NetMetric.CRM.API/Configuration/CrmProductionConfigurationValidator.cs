// <copyright file="CrmProductionConfigurationValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Configuration;

public static class CrmProductionConfigurationValidator
{
    private static readonly string[] RequiredConnectionStrings =
    [
        "DefaultConnection",
        "AnalyticsReportingConnection",
        "ArtificialIntelligenceConnection",
        "CalendarSyncConnection",
        "ContractLifecycleConnection",
        "CustomerIntelligenceConnection",
        "CustomerManagementConnection",
        "DealManagementConnection",
        "DocumentManagementConnection",
        "FinanceOperationsConnection",
        "IntegrationHubConnection",
        "KnowledgeBaseManagementConnection",
        "LeadManagementConnection",
        "MarketingAutomationConnection",
        "OmnichannelConnection",
        "OpportunityManagementConnection",
        "PipelineManagementConnection",
        "ProductCatalogConnection",
        "QuoteManagementConnection",
        "SalesForecastingConnection",
        "SupportInboxIntegrationConnection",
        "TagManagementConnection",
        "TenantManagementConnection",
        "TicketManagementConnection",
        "TicketSlaManagementConnection",
        "TicketWorkflowManagementConnection",
        "WorkflowAutomationConnection",
        "WorkManagementConnection"
    ];

    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            return;
        }

        var errors = new List<string>();
        ValidateHosts(configuration, errors);
        ValidateJwt(configuration, errors);
        ValidateCors(configuration, errors);
        ValidateConnectionStrings(configuration, errors);
        ValidateForwardedHeaders(configuration, errors);
        ValidateOperationalDependencies(configuration, errors);
        ValidateMessaging(configuration, errors);
        ValidateTrafficControls(configuration, errors);

        if (errors.Count > 0)
        {
            throw new InvalidOperationException("CRM production configuration is invalid: " + string.Join(" ", errors));
        }
    }

    private static void ValidateHosts(IConfiguration configuration, ICollection<string> errors)
    {
        var allowedHosts = configuration["AllowedHosts"];
        var hosts = allowedHosts?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (string.IsNullOrWhiteSpace(allowedHosts) ||
            hosts.Contains("*") ||
            hosts.Any(IsPlaceholderHost))
        {
            errors.Add("AllowedHosts must name the production host(s); wildcard, localhost and example hosts are not allowed.");
        }
    }

    private static void ValidateJwt(IConfiguration configuration, ICollection<string> errors)
    {
        var jwtSection = configuration.GetSection("Authentication:Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var metadataAddress = jwtSection["MetadataAddress"];
        var authority = jwtSection["Authority"];

        if (string.IsNullOrWhiteSpace(issuer) ||
            string.IsNullOrWhiteSpace(audience) ||
            (string.IsNullOrWhiteSpace(metadataAddress) && string.IsNullOrWhiteSpace(authority)))
        {
            errors.Add("Authentication:Jwt issuer, audience and metadata address or authority must be configured.");
            return;
        }

        if (!IsHttpsUri(issuer) || !IsHttpsUri(audience) || IsPlaceholderUri(issuer) || IsPlaceholderUri(audience))
        {
            errors.Add("Authentication:Jwt issuer and audience must be production HTTPS URLs.");
        }

        if (!string.IsNullOrWhiteSpace(metadataAddress) && (!IsHttpsUri(metadataAddress) || IsPlaceholderUri(metadataAddress)))
        {
            errors.Add("Authentication:Jwt metadata address must be a production HTTPS URL.");
        }

        if (!string.IsNullOrWhiteSpace(authority) && (!IsHttpsUri(authority) || IsPlaceholderUri(authority)))
        {
            errors.Add("Authentication:Jwt authority must be a production HTTPS URL.");
        }
    }

    private static void ValidateCors(IConfiguration configuration, ICollection<string> errors)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0)
        {
            errors.Add("Cors:AllowedOrigins must include at least one HTTPS origin.");
            return;
        }

        if (origins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
                                  !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                                  !string.IsNullOrEmpty(uri.Query) ||
                                  !string.IsNullOrEmpty(uri.Fragment) ||
                                  (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/") ||
                                  IsPlaceholderHost(uri.Host)))
        {
            errors.Add("Cors:AllowedOrigins entries must be production HTTPS origins.");
        }
    }

    private static void ValidateConnectionStrings(IConfiguration configuration, ICollection<string> errors)
    {
        foreach (var name in RequiredConnectionStrings)
        {
            var value = configuration.GetConnectionString(name);
            if (string.IsNullOrWhiteSpace(value))
            {
                errors.Add($"{name} must be configured.");
                continue;
            }

            if (value.Contains("(localdb)", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                value.Contains(".example.", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("example.com", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("TrustServerCertificate=True", StringComparison.OrdinalIgnoreCase) ||
                value.Contains("Encrypt=False", StringComparison.OrdinalIgnoreCase))
            {
                errors.Add($"{name} must not use localdb, localhost, example hosts, disabled encryption or TrustServerCertificate=True in production.");
            }
        }
    }

    private static void ValidateForwardedHeaders(IConfiguration configuration, ICollection<string> errors)
    {
        var knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];
        var knownNetworks = configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [];
        if (knownProxies.Length == 0 && knownNetworks.Length == 0)
        {
            errors.Add("ForwardedHeaders must configure KnownProxies or KnownNetworks in production.");
        }

        if (knownProxies.Any(proxy => !System.Net.IPAddress.TryParse(proxy, out _)))
        {
            errors.Add("ForwardedHeaders:KnownProxies entries must be valid IP addresses.");
        }

        if (knownNetworks.Any(network => !TryParseNetwork(network)))
        {
            errors.Add("ForwardedHeaders:KnownNetworks entries must be valid CIDR ranges.");
        }
    }

    private static void ValidateOperationalDependencies(IConfiguration configuration, ICollection<string> errors)
    {
        if (string.IsNullOrWhiteSpace(configuration.GetConnectionString("Redis")))
        {
            errors.Add("Redis connection string is required in production for distributed idempotency and cache-backed coordination.");
        }
        else if (configuration.GetConnectionString("Redis")!.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                 configuration.GetConnectionString("Redis")!.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                 configuration.GetConnectionString("Redis")!.Contains("example", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Redis connection string must point at a production cache endpoint.");
        }

        var dataProtectionKeysPath = configuration["DataProtection:KeysPath"] ?? configuration["Crm:DataProtection:KeysPath"];
        if (string.IsNullOrWhiteSpace(dataProtectionKeysPath) || !Path.IsPathRooted(dataProtectionKeysPath))
        {
            errors.Add("DataProtection:KeysPath must be configured as an absolute persistent volume path in production.");
        }
    }

    private static void ValidateMessaging(IConfiguration configuration, ICollection<string> errors)
    {
        var rabbitMqUri = configuration["Messaging:RabbitMq:Uri"];
        if (string.IsNullOrWhiteSpace(rabbitMqUri) ||
            !Uri.TryCreate(rabbitMqUri, UriKind.Absolute, out var uri))
        {
            errors.Add("Messaging:RabbitMq:Uri must be configured as an amqps URI in production.");
            return;
        }

        if (uri.Scheme != "amqps" ||
            uri.IsLoopback ||
            string.IsNullOrWhiteSpace(uri.UserInfo) ||
            uri.UserInfo.Contains("guest", StringComparison.OrdinalIgnoreCase) ||
            uri.UserInfo.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
            uri.UserInfo.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
            uri.UserInfo.Contains("TEST", StringComparison.OrdinalIgnoreCase) ||
            uri.UserInfo.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
            uri.UserInfo.Contains("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Messaging:RabbitMq:Uri must use amqps, production broker credentials and a non-loopback host.");
        }

        var exchange = configuration["Messaging:RabbitMq:Exchange"];
        if (string.IsNullOrWhiteSpace(exchange))
        {
            errors.Add("Messaging:RabbitMq:Exchange is required in production.");
        }
        else if (exchange.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
                 exchange.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
                 exchange.Contains("TEST", StringComparison.OrdinalIgnoreCase) ||
                 exchange.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
                 exchange.Contains("REPLACE", StringComparison.OrdinalIgnoreCase))
        {
            errors.Add("Messaging:RabbitMq:Exchange must not contain local/dev/test markers in production.");
        }
    }

    private static void ValidateTrafficControls(IConfiguration configuration, ICollection<string> errors)
    {
        var maxRequestBodyBytes = configuration.GetValue<long?>("Http:MaxRequestBodyBytes");
        if (maxRequestBodyBytes is null or <= 0 or > 25_000_000)
        {
            errors.Add("Http:MaxRequestBodyBytes must be configured between 1 byte and 25 MB in production.");
        }

        var permitLimit = configuration.GetValue<int?>("RateLimiting:PermitLimit");
        if (permitLimit is null or <= 0 or > 10_000)
        {
            errors.Add("RateLimiting:PermitLimit must be configured between 1 and 10000 requests per minute in production.");
        }

        ValidatePublicRateLimit(configuration, errors, "RateLimiting:PublicCapture", 1, 1_000);
        ValidatePublicRateLimit(configuration, errors, "RateLimiting:MarketingConsent", 1, 1_000);
        ValidatePublicRateLimit(configuration, errors, "RateLimiting:Webhooks", 1, 10_000);

        var captureAllowedOrigins = configuration.GetSection("Crm:PublicCapture:AllowedOrigins").Get<string[]>() ?? [];
        if (captureAllowedOrigins.Length == 0)
        {
            errors.Add("Crm:PublicCapture:AllowedOrigins must contain at least one trusted HTTPS origin in production.");
        }
        else if (captureAllowedOrigins.Any(origin => !IsHttpsUri(origin) || IsPlaceholderUri(origin)))
        {
            errors.Add("Crm:PublicCapture:AllowedOrigins must contain only trusted production HTTPS origins.");
        }

        var consentSigningKey = configuration["Crm:MarketingAutomation:ConsentTokens:SigningKey"];
        if (string.IsNullOrWhiteSpace(consentSigningKey) || consentSigningKey.Length < 32)
        {
            errors.Add("Crm:MarketingAutomation:ConsentTokens:SigningKey must be configured with at least 32 characters in production.");
        }
    }

    private static void ValidatePublicRateLimit(
        IConfiguration configuration,
        ICollection<string> errors,
        string section,
        int minimumPermitLimit,
        int maximumPermitLimit)
    {
        var permitLimit = configuration.GetValue<int?>($"{section}:PermitLimit");
        if (permitLimit is null || permitLimit < minimumPermitLimit || permitLimit > maximumPermitLimit)
        {
            errors.Add($"{section}:PermitLimit must be configured between {minimumPermitLimit} and {maximumPermitLimit} in production.");
        }

        var windowSeconds = configuration.GetValue<int?>($"{section}:WindowSeconds");
        if (windowSeconds is null or <= 0 or > 3600)
        {
            errors.Add($"{section}:WindowSeconds must be configured between 1 and 3600 seconds in production.");
        }
    }

    private static bool IsHttpsUri(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
        string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);

    private static bool IsPlaceholderUri(string value)
        => Uri.TryCreate(value, UriKind.Absolute, out var uri) && IsPlaceholderHost(uri.Host);

    private static bool IsPlaceholderHost(string host)
        => host.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
           host.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
           host.Contains(".example.", StringComparison.OrdinalIgnoreCase) ||
           host.Equals("example.com", StringComparison.OrdinalIgnoreCase) ||
           host.EndsWith(".example.com", StringComparison.OrdinalIgnoreCase);

    private static bool TryParseNetwork(string value)
    {
        var segments = value.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return segments.Length == 2 &&
               System.Net.IPAddress.TryParse(segments[0], out _) &&
               int.TryParse(segments[1], out var prefixLength) &&
               prefixLength is >= 0 and <= 128;
    }
}
