// <copyright file="AccountOperationalHardeningExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetMetric.Account.Api.Middleware;
using NetMetric.Account.Api.RateLimiting;
using NetMetric.Account.Api.Security;
using NetMetric.AspNetCore.RequestContext;
using NetMetric.AspNetCore.Security;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace NetMetric.Account.Api.DependencyInjection;

public static class AccountOperationalHardeningExtensions
{
    public const string GlobalRateLimitPolicy = "account-global";
    public const string CriticalRateLimitPolicy = "account-critical";

    public static IServiceCollection AddAccountOperationalHardening(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddValidatedOptions();
        services.AddAccountAuthentication(configuration, environment);
        services.AddAccountForwardedHeaders();
        services.AddAccountRateLimiting(configuration);
        services.AddAccountOpenTelemetry(configuration);

        return services;
    }

    public static IApplicationBuilder UseAccountOperationalHardening(this WebApplication app)
    {
        var securityHeaders = app.Services.GetRequiredService<IOptions<AccountSecurityHeadersOptions>>().Value;
        if (!app.Environment.IsDevelopment() && securityHeaders.EnableHsts)
        {
            app.UseHsts();
        }

        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseRateLimiter();

        return app;
    }

    private static IServiceCollection AddValidatedOptions(this IServiceCollection services)
    {
        services
            .AddOptions<AccountJwtOptions>()
            .BindConfiguration(AccountJwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Authority), "JWT authority is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "JWT issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "JWT audience is required.")
            .Validate(options => options.ClockSkewSeconds >= 0 && options.ClockSkewSeconds <= 300, "JWT clock skew must be between 0 and 300 seconds.")
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountJwtOptions>, AccountJwtOptionsValidation>();

        services
            .AddOptions<AccountSecurityHeadersOptions>()
            .BindConfiguration(AccountSecurityHeadersOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountSecurityHeadersOptions>, AccountSecurityHeadersOptionsValidation>();

        services
            .AddOptions<AccountForwardedHeadersOptions>()
            .BindConfiguration(AccountForwardedHeadersOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountForwardedHeadersOptions>, AccountForwardedHeadersOptionsValidation>();

        services
            .AddOptions<AccountRateLimitingOptions>()
            .BindConfiguration(AccountRateLimitingOptions.SectionName)
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountRateLimitingOptions>, AccountRateLimitingOptionsValidation>();

        return services;
    }

    private static IServiceCollection AddAccountAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var jwt = configuration.GetSection(AccountJwtOptions.SectionName).Get<AccountJwtOptions>() ?? new AccountJwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.Authority = jwt.Authority;
                if (!string.IsNullOrWhiteSpace(jwt.MetadataAddress))
                {
                    options.MetadataAddress = jwt.MetadataAddress;
                }
                options.Audience = jwt.Audience;
                options.RequireHttpsMetadata = jwt.RequireHttpsMetadata && !environment.IsDevelopment();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(jwt.ClockSkewSeconds)
                };
            });

        return services;
    }

    private static IServiceCollection AddAccountForwardedHeaders(this IServiceCollection services)
    {
        services.AddOptions<ForwardedHeadersOptions>()
            .Configure<IOptions<AccountForwardedHeadersOptions>>((options, accountOptions) =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
                options.ForwardLimit = accountOptions.Value.ForwardLimit;

                foreach (var proxy in accountOptions.Value.KnownProxies)
                {
                    if (IPAddress.TryParse(proxy, out var address))
                    {
                        options.KnownProxies.Add(address);
                    }
                }

                foreach (var network in accountOptions.Value.KnownNetworks)
                {
                    if (SecuritySupport.TryParseNetwork(network, out var parsedNetwork))
                    {
                        options.KnownIPNetworks.Add(parsedNetwork);
                    }
                }
            });

        return services;
    }

    private static IServiceCollection AddAccountRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var options = configuration.GetSection(AccountRateLimitingOptions.SectionName).Get<AccountRateLimitingOptions>() ?? new AccountRateLimitingOptions();
        var environmentName = configuration["ASPNETCORE_ENVIRONMENT"]?.ToLowerInvariant() ?? "unknown";

        services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            rateLimiter.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;
                var policy = context.Lease?.TryGetMetadata(MetadataName.RetryAfter, out _) == true ? CriticalRateLimitPolicy : GlobalRateLimitPolicy;
                var tenantHash = HashForTag(httpContext.User.FindFirst("tenant_id")?.Value ?? httpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault());
                AccountRateLimitMetrics.Rejected.Add(
                    1,
                    new KeyValuePair<string, object?>("endpoint", httpContext.Request.Path.Value),
                    new KeyValuePair<string, object?>("policy", policy),
                    new KeyValuePair<string, object?>("reason", "rejected"),
                    new KeyValuePair<string, object?>("tenant_hash", tenantHash),
                    new KeyValuePair<string, object?>("environment", environmentName));

                httpContext.Response.Headers.RetryAfter = options.Critical.WindowSeconds.ToString();
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await httpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        type = "https://httpstatuses.com/429",
                        title = "Rate limit exceeded",
                        status = StatusCodes.Status429TooManyRequests,
                        detail = "Too many account requests were sent.",
                        traceId = httpContext.TraceIdentifier,
                        correlationId = RequestContextSupport.GetOrCreateCorrelationId(httpContext),
                        errorCode = "account_rate_limit_exceeded"
                    },
                    cancellationToken);
            };

            rateLimiter.AddPolicy(GlobalRateLimitPolicy, context =>
            {
                var key = context.User.FindFirst("sub")?.Value ??
                    context.Connection.RemoteIpAddress?.ToString() ??
                    "anonymous";

                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.Global.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.Global.WindowSeconds),
                        QueueLimit = options.Global.QueueLimit
                    });
            });

            rateLimiter.AddPolicy(CriticalRateLimitPolicy, context =>
            {
                var key = $"{context.User.FindFirst("sub")?.Value ?? "anonymous"}:{context.Connection.RemoteIpAddress}";
                return RateLimitPartition.GetFixedWindowLimiter(
                    key,
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.Critical.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.Critical.WindowSeconds),
                        QueueLimit = options.Critical.QueueLimit
                    });
            });
        });

        return services;
    }

    private static IServiceCollection AddAccountOpenTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("NetMetric.Account.Api"))
            .WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();
                tracing.AddEntityFrameworkCoreInstrumentation();

                var endpoint = configuration["OpenTelemetry:Otlp:Endpoint"] ?? configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddPrometheusExporter();

                var endpoint = configuration["OpenTelemetry:Otlp:Endpoint"] ?? configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(endpoint));
                }
            });

        return services;
    }

    private static string HashForTag(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "anonymous";
        }

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes[..6]).ToLowerInvariant();
    }
}

internal static class AccountRateLimitMetrics
{
    private static readonly Meter Meter = new("NetMetric.Account.Api");
    public static readonly Counter<long> Rejected = Meter.CreateCounter<long>(
        "account.rate_limit.rejected",
        description: "Number of requests rejected by account rate limiting.");
}
