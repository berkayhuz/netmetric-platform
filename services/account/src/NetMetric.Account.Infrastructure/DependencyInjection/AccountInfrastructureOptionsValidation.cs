// <copyright file="AccountInfrastructureOptionsValidation.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetMetric.Account.Infrastructure.Identity;
using NetMetric.Account.Infrastructure.IntegrationEvents;
using NetMetric.Account.Infrastructure.Membership;
using NetMetric.Account.Infrastructure.Outbox;
using NetMetric.AspNetCore.TrustedGateway.Options;

namespace NetMetric.Account.Infrastructure.DependencyInjection;

public sealed class IdentityServiceOptionsValidation(IHostEnvironment environment) : IValidateOptions<IdentityServiceOptions>
{
    public ValidateOptionsResult Validate(string? name, IdentityServiceOptions options)
    {
        var failures = new List<string>();
        ValidateServiceUrl(options.BaseUrl, "IdentityService:BaseUrl", failures);

        if (options.TimeoutSeconds <= 0)
        {
            failures.Add("IdentityService:TimeoutSeconds must be greater than zero.");
        }

        if (options.GetRetryCount < 0)
        {
            failures.Add("IdentityService:GetRetryCount must not be negative.");
        }

        if (options.GetRetryDelayMilliseconds <= 0)
        {
            failures.Add("IdentityService:GetRetryDelayMilliseconds must be greater than zero.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private void ValidateServiceUrl(string value, string key, ICollection<string> failures)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && !IsDevelopmentHttpLoopback(uri)))
        {
            failures.Add($"{key} must be an absolute HTTPS URL.");
            return;
        }

        if (environment.IsProduction() && (uri.IsLoopback || ContainsUnsafeMarker(uri.Host)))
        {
            failures.Add($"{key} must point to a production HTTPS endpoint.");
        }
    }

    private bool IsDevelopmentHttpLoopback(Uri uri) =>
        environment.IsDevelopment() &&
        uri.Scheme == Uri.UriSchemeHttp &&
        uri.IsLoopback;

    private static bool ContainsUnsafeMarker(string value) =>
        value.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("REPLACE", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("TEST", StringComparison.OrdinalIgnoreCase);
}

public sealed class MembershipServiceOptionsValidation(IHostEnvironment environment) : IValidateOptions<MembershipServiceOptions>
{
    public ValidateOptionsResult Validate(string? name, MembershipServiceOptions options)
    {
        var failures = new List<string>();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && !IsDevelopmentHttpLoopback(uri)))
        {
            failures.Add("MembershipService:BaseUrl must be an absolute HTTPS URL.");
        }
        else if (environment.IsProduction() && (uri.IsLoopback || ContainsUnsafeMarker(uri.Host)))
        {
            failures.Add("MembershipService:BaseUrl must point to a production HTTPS endpoint.");
        }

        if (environment.IsProduction() && options.SkipRemoteCalls)
        {
            failures.Add("MembershipService:SkipRemoteCalls must be false in production.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            failures.Add("MembershipService:TimeoutSeconds must be greater than zero.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }

    private bool IsDevelopmentHttpLoopback(Uri uri) =>
        environment.IsDevelopment() &&
        uri.Scheme == Uri.UriSchemeHttp &&
        uri.IsLoopback;

    private static bool ContainsUnsafeMarker(string value) =>
        value.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("REPLACE", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("CHANGE_ME", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
        value.Contains("TEST", StringComparison.OrdinalIgnoreCase);
}

public sealed class AccountTrustedGatewayOptionsValidation(IHostEnvironment environment) : IValidateOptions<TrustedGatewayOptions>
{
    private static readonly string[] UnsafeSecretMarkers = ["REPLACE", "CHANGE_ME", "LOCAL", "DEV", "TEST", "CHANGETHIS"];

    public ValidateOptionsResult Validate(string? name, TrustedGatewayOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Source))
        {
            failures.Add("Security:TrustedGateway:Source is required.");
        }

        if (string.IsNullOrWhiteSpace(options.CurrentKeyId))
        {
            failures.Add("Security:TrustedGateway:CurrentKeyId is required.");
        }

        if (!options.Keys.Any(key => key.Enabled && key.SignRequests && string.Equals(key.KeyId, options.CurrentKeyId, StringComparison.Ordinal)))
        {
            failures.Add("Security:TrustedGateway:CurrentKeyId must reference an enabled signing key.");
        }

        foreach (var key in options.Keys)
        {
            if (string.IsNullOrWhiteSpace(key.KeyId))
            {
                failures.Add("Security:TrustedGateway:Keys:KeyId is required.");
            }

            if (environment.IsProduction())
            {
                if (string.IsNullOrWhiteSpace(key.Secret))
                {
                    failures.Add($"Security:TrustedGateway:Keys:{key.KeyId}:Secret is required in production.");
                }

                if ((key.Secret?.Length ?? 0) < 32)
                {
                    failures.Add($"Security:TrustedGateway:Keys:{key.KeyId}:Secret must be at least 32 characters in production.");
                }

                if (!string.IsNullOrWhiteSpace(key.Secret) &&
                    UnsafeSecretMarkers.Any(marker => key.Secret.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    failures.Add($"Security:TrustedGateway:Keys:{key.KeyId}:Secret contains a non-production placeholder marker.");
                }

                if (!string.IsNullOrWhiteSpace(key.KeyId) &&
                    (key.KeyId.Contains("local", StringComparison.OrdinalIgnoreCase) ||
                     key.KeyId.Contains("dev", StringComparison.OrdinalIgnoreCase) ||
                     key.KeyId.Contains("test", StringComparison.OrdinalIgnoreCase)))
                {
                    failures.Add($"Security:TrustedGateway:Keys:{key.KeyId}:KeyId cannot be a local/dev/test identifier in production.");
                }
            }
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}

public sealed class AccountOutboxOptionsValidation(IHostEnvironment environment) : IValidateOptions<AccountOutboxOptions>
{
    public ValidateOptionsResult Validate(string? name, AccountOutboxOptions options)
    {
        var failures = new List<string>();

        if (environment.IsProduction() && !options.EnableProcessor)
        {
            failures.Add("Outbox:EnableProcessor must be true in production so account security notifications are not dropped.");
        }

        if (options.PollingIntervalSeconds <= 0)
        {
            failures.Add("Outbox:PollingIntervalSeconds must be greater than zero.");
        }

        if (options.BatchSize <= 0)
        {
            failures.Add("Outbox:BatchSize must be greater than zero.");
        }

        if (options.MaxAttempts < 1)
        {
            failures.Add("Outbox:MaxAttempts must be at least one.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}

public sealed class AuthProfileBootstrapOptionsValidation(IHostEnvironment environment) : IValidateOptions<AuthProfileBootstrapOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthProfileBootstrapOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.QueueName))
        {
            failures.Add("AuthProfileBootstrap:QueueName is required.");
        }

        if (environment.IsProduction() && !options.Enabled)
        {
            failures.Add("AuthProfileBootstrap:Enabled must be true in production so Account profiles are provisioned after registration.");
        }

        return failures.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(failures);
    }
}
