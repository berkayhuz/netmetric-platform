// <copyright file="NotificationInfrastructureOptionsValidation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.Notification.Infrastructure.Integration;
using NetMetric.Notification.Infrastructure.Modules.Email.Infrastructure.Options;
using NetMetric.Notification.Infrastructure.Options;

namespace NetMetric.Notification.Infrastructure.DependencyInjection;

public sealed class NotificationDatabaseOptionsValidation(
    IHostEnvironment environment,
    IConfiguration configuration) : IValidateOptions<NotificationDatabaseOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationDatabaseOptions options)
    {
        if (!environment.IsProduction())
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();
        if (options.ApplyMigrationsOnStartup)
        {
            failures.Add("Notification:Database:ApplyMigrationsOnStartup must be false in production.");
        }

        var connectionString = configuration.GetConnectionString("NotificationConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            failures.Add("ConnectionStrings:NotificationConnection is required in production.");
        }
        else
        {
            ValidateSqlConnectionString(connectionString, failures);
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateSqlConnectionString(string connectionString, ICollection<string> failures)
    {
        SqlConnectionStringBuilder builder;
        try
        {
            builder = new SqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException exception)
        {
            failures.Add($"ConnectionStrings:NotificationConnection is invalid: {exception.Message}");
            return;
        }

        if (IsUnsafeHost(ExtractSqlHost(builder.DataSource)))
        {
            failures.Add("ConnectionStrings:NotificationConnection must not use localhost, loopback, localdb, or marker hosts in production.");
        }

        if (builder.TrustServerCertificate)
        {
            failures.Add("ConnectionStrings:NotificationConnection must not set TrustServerCertificate=True in production.");
        }

        if (string.Equals(builder.UserID, "sa", StringComparison.OrdinalIgnoreCase))
        {
            failures.Add("ConnectionStrings:NotificationConnection must not use the sa login in production.");
        }

        if (!string.IsNullOrWhiteSpace(builder.Password) &&
            (builder.Password.Length < 16 || ContainsUnsafeMarker(builder.Password)))
        {
            failures.Add("ConnectionStrings:NotificationConnection must use a strong production database password.");
        }
    }

    private static string ExtractSqlHost(string dataSource)
    {
        var host = dataSource.Trim();
        if (host.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
        {
            host = host[4..];
        }

        var commaIndex = host.IndexOf(',');
        if (commaIndex >= 0)
        {
            host = host[..commaIndex];
        }

        var slashIndex = host.IndexOf('\\');
        if (slashIndex >= 0)
        {
            host = host[..slashIndex];
        }

        return host;
    }

    private static bool IsUnsafeHost(string host) =>
        string.IsNullOrWhiteSpace(host) ||
        string.Equals(host, ".", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(host, "(local)", StringComparison.OrdinalIgnoreCase) ||
        host.Contains("localdb", StringComparison.OrdinalIgnoreCase) ||
        Uri.CheckHostName(host) == UriHostNameType.Unknown ||
        ContainsUnsafeMarker(host);

    private static bool ContainsUnsafeMarker(string value)
    {
        var markers = new[] { "localhost", "127.0.0.1", "::1", "change_me", "replace", "local", "dev", "test" };
        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class NotificationRabbitMqOptionsValidation(IHostEnvironment environment) : IValidateOptions<NotificationRabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationRabbitMqOptions options)
    {
        if (!environment.IsProduction())
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();
        if (IsUnsafeHost(options.Host))
        {
            failures.Add("Notification:RabbitMq:Host must be a production broker host.");
        }

        if (!options.UseTls)
        {
            failures.Add("Notification:RabbitMq:UseTls must be true in production.");
        }

        if (options.Port == 5672)
        {
            failures.Add("Notification:RabbitMq:Port must not use the non-TLS AMQP port in production.");
        }

        if (string.Equals(options.Username, "guest", StringComparison.OrdinalIgnoreCase) ||
            ContainsUnsafeMarker(options.Username))
        {
            failures.Add("Notification:RabbitMq:Username must not be guest or contain local/dev/test markers in production.");
        }

        if (string.IsNullOrWhiteSpace(options.Password) ||
            options.Password.Length < 16 ||
            string.Equals(options.Password, "guest", StringComparison.OrdinalIgnoreCase) ||
            ContainsUnsafeMarker(options.Password))
        {
            failures.Add("Notification:RabbitMq:Password must be a strong production secret.");
        }

        if (!options.UseQuorumQueue)
        {
            failures.Add("Notification:RabbitMq:UseQuorumQueue must be true in production.");
        }

        ValidateName(options.QueueName, "Notification:RabbitMq:QueueName", failures);
        ValidateName(options.DeadLetterExchangeName, "Notification:RabbitMq:DeadLetterExchangeName", failures);
        ValidateName(options.DeadLetterQueueName, "Notification:RabbitMq:DeadLetterQueueName", failures);
        ValidateName(options.DeadLetterRoutingKey, "Notification:RabbitMq:DeadLetterRoutingKey", failures);

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateName(string value, string settingName, ICollection<string> failures)
    {
        if (string.IsNullOrWhiteSpace(value) || ContainsUnsafeMarker(value))
        {
            failures.Add($"{settingName} must be configured without local/dev/test markers in production.");
        }
    }

    private static bool IsUnsafeHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || ContainsUnsafeMarker(host))
        {
            return true;
        }

        return Uri.CheckHostName(host) == UriHostNameType.Unknown;
    }

    private static bool ContainsUnsafeMarker(string value)
    {
        var markers = new[] { "localhost", "127.0.0.1", "::1", "change_me", "replace", "local", "dev", "test" };
        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class NotificationIntegrationConsumerOptionsValidation(IHostEnvironment environment)
    : IValidateOptions<NotificationIntegrationConsumerOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationIntegrationConsumerOptions options)
    {
        if (!environment.IsProduction())
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();
        if (!options.Enabled)
        {
            failures.Add("Notification:IntegrationConsumer:Enabled must be true in production so account and security notifications are consumed.");
        }

        if (string.IsNullOrWhiteSpace(options.QueueName) || ContainsUnsafeMarker(options.QueueName))
        {
            failures.Add("Notification:IntegrationConsumer:QueueName must be configured without local/dev/test markers in production.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool ContainsUnsafeMarker(string value)
    {
        var markers = new[] { "localhost", "127.0.0.1", "::1", "change_me", "replace", "local", "dev", "test" };
        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class NotificationEmailProviderOptionsValidation : IValidateOptions<EmailProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, EmailProviderOptions options)
    {
        var provider = options.Provider.Trim().ToLowerInvariant();
        return provider is "smtp" or "ses" or "amazon-ses" or "amazonses"
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail("Notification:Email:Provider must be smtp or ses.");
    }
}

public sealed class SmtpEmailProviderOptionsValidation(
    IHostEnvironment environment,
    IOptions<EmailProviderOptions> providerOptions) : IValidateOptions<SmtpEmailProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, SmtpEmailProviderOptions options)
    {
        if (!UsesSmtp(providerOptions.Value.Provider))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Host) || options.Port is < 1 or > 65535)
        {
            failures.Add("Notification:Email:Smtp host and port are required when SMTP is selected.");
        }

        if (string.IsNullOrWhiteSpace(options.FromAddress) || !IsEmailLike(options.FromAddress))
        {
            failures.Add("Notification:Email:Smtp:FromAddress must be a valid sender address.");
        }

        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            failures.Add("Notification:Email:Smtp:FromName is required.");
        }

        if (!string.IsNullOrWhiteSpace(options.UserName) && string.IsNullOrWhiteSpace(options.Password))
        {
            failures.Add("Notification:Email:Smtp:Password is required when UserName is configured.");
        }

        if (environment.IsProduction())
        {
            if (IsUnsafeHost(options.Host))
            {
                failures.Add("Notification:Email:Smtp:Host must be a production SMTP host.");
            }

            if (!options.UseStartTls)
            {
                failures.Add("Notification:Email:Smtp:UseStartTls must be true in production.");
            }

            if (string.IsNullOrWhiteSpace(options.UserName) || string.IsNullOrWhiteSpace(options.Password))
            {
                failures.Add("Notification:Email:Smtp requires credentials in production.");
            }

            if (ContainsUnsafeMarker(options.FromAddress) ||
                ContainsUnsafeMarker(options.FromName) ||
                ContainsUnsafeMarker(options.UserName ?? string.Empty) ||
                ContainsUnsafeMarker(options.Password ?? string.Empty))
            {
                failures.Add("Notification:Email:Smtp settings must not contain local/dev/test markers in production.");
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool UsesSmtp(string provider) =>
        provider.Trim().Equals("smtp", StringComparison.OrdinalIgnoreCase);

    private static bool IsEmailLike(string value) =>
        value.Contains('@', StringComparison.Ordinal) &&
        value.Contains('.', StringComparison.Ordinal);

    private static bool IsUnsafeHost(string value) =>
        string.IsNullOrWhiteSpace(value) ||
        Uri.CheckHostName(value) == UriHostNameType.Unknown ||
        ContainsUnsafeMarker(value);

    private static bool ContainsUnsafeMarker(string value)
    {
        var markers = new[] { "localhost", "127.0.0.1", "::1", "change_me", "replace", "local", "dev", "test" };
        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}

public sealed class AmazonSesEmailProviderOptionsValidation(
    IHostEnvironment environment,
    IOptions<EmailProviderOptions> providerOptions) : IValidateOptions<AmazonSesEmailProviderOptions>
{
    public ValidateOptionsResult Validate(string? name, AmazonSesEmailProviderOptions options)
    {
        if (!UsesSes(providerOptions.Value.Provider))
        {
            return ValidateOptionsResult.Success;
        }

        var failures = new List<string>();
        if (string.IsNullOrWhiteSpace(options.Region))
        {
            failures.Add("Notification:Email:AmazonSes:Region is required when SES is selected.");
        }

        if (string.IsNullOrWhiteSpace(options.FromAddress) || !IsEmailLike(options.FromAddress))
        {
            failures.Add("Notification:Email:AmazonSes:FromAddress must be a valid sender address.");
        }

        if (string.IsNullOrWhiteSpace(options.FromName))
        {
            failures.Add("Notification:Email:AmazonSes:FromName is required.");
        }

        if (environment.IsProduction())
        {
            if (ContainsUnsafeMarker(options.Region) ||
                ContainsUnsafeMarker(options.FromAddress) ||
                ContainsUnsafeMarker(options.FromName) ||
                ContainsUnsafeMarker(options.AccessKeyId ?? string.Empty) ||
                ContainsUnsafeMarker(options.SecretAccessKey ?? string.Empty))
            {
                failures.Add("Notification:Email:AmazonSes settings must not contain local/dev/test markers in production.");
            }

            if (!string.IsNullOrWhiteSpace(options.EndpointUrl) &&
                (!Uri.TryCreate(options.EndpointUrl, UriKind.Absolute, out var endpoint) ||
                 endpoint.Scheme != Uri.UriSchemeHttps ||
                 ContainsUnsafeMarker(endpoint.Host)))
            {
                failures.Add("Notification:Email:AmazonSes:EndpointUrl must be an HTTPS production endpoint when configured.");
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private static bool UsesSes(string provider)
    {
        var normalized = provider.Trim().ToLowerInvariant();
        return normalized is "ses" or "amazon-ses" or "amazonses";
    }

    private static bool IsEmailLike(string value) =>
        value.Contains('@', StringComparison.Ordinal) &&
        value.Contains('.', StringComparison.Ordinal);

    private static bool ContainsUnsafeMarker(string value)
    {
        var markers = new[] { "localhost", "127.0.0.1", "::1", "change_me", "replace", "local", "dev", "test" };
        return markers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }
}
