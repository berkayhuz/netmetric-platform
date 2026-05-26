// <copyright file="Program.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NetMetric.AspNetCore.Health;
using NetMetric.AspNetCore.ProblemDetails;
using NetMetric.AspNetCore.Security;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;
using NetMetric.CRM.API.Compatibility;
using NetMetric.CRM.API.Configuration;
using NetMetric.CRM.API.Controllers.Integrations;
using NetMetric.CRM.API.DependencyInjection;
using NetMetric.CRM.API.Middleware;
using NetMetric.CRM.API.Security;
using NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;
using NetMetric.CRM.IntegrationHub.Infrastructure.Processing;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var crmRateLimitCounter = new System.Diagnostics.Metrics.Meter("NetMetric.CRM.API")
    .CreateCounter<long>("crm.rate_limit.rejected", description: "Number of requests rejected by CRM rate limiting.");

CrmProductionConfigurationValidator.Validate(builder.Configuration, builder.Environment);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = builder.Configuration.GetValue<long?>("Http:MaxRequestBodyBytes") ?? 10_485_760;
});

builder.Services.AddNetMetricProblemDetails();
builder.Services.AddResponseCompression();
builder.Services.AddScoped<TenantRouteGuardFilter>();
builder.Services.AddScoped<IntegrationHubControllerServices>();
builder.Services.AddSingleton<IPublicCaptureChallengeVerifier, NoopPublicCaptureChallengeVerifier>();
builder.Services.AddControllers(options => options.Filters.Add<TenantRouteGuardFilter>());
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
var dataProtectionBuilder = builder.Services.AddDataProtection()
    .SetApplicationName("NetMetric.CRM");
var dataProtectionKeysPath =
    builder.Configuration["DataProtection:KeysPath"] ??
    builder.Configuration["Crm:DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(dataProtectionKeysPath))
{
    Directory.CreateDirectory(dataProtectionKeysPath);
    dataProtectionBuilder.PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeysPath));
}
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "crm:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("NetMetric.CRM.API"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation();

        var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"] ??
            builder.Configuration["OpenTelemetry:OtlpEndpoint"] ??
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter("NetMetric.CRM.API")
            .AddMeter(AnalyticsProjectionMetrics.MeterName)
            .AddMeter(CustomerManagementOutboxMetrics.MeterName)
            .AddMeter(IntegrationJobMetrics.MeterName)
            .AddMeter(WorkflowAutomationMetrics.MeterName)
            .AddPrometheusExporter();

        var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"] ??
            builder.Configuration["OpenTelemetry:OtlpEndpoint"] ??
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    });

builder.Services.AddNetMetricCrm(builder.Configuration, builder.Environment);
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.ForwardLimit = builder.Configuration.GetValue<int?>("ForwardedHeaders:ForwardLimit") ?? 1;
    options.RequireHeaderSymmetry = true;

    foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
    {
        if (IPAddress.TryParse(proxy, out var address))
        {
            options.KnownProxies.Add(address);
        }
    }

    foreach (var network in builder.Configuration.GetSection("ForwardedHeaders:KnownNetworks").Get<string[]>() ?? [])
    {
        if (System.Net.IPNetwork.TryParse(network, out var parsedNetwork))
        {
            options.KnownIPNetworks.Add(parsedNetwork);
        }
    }
});
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
    options.AddPolicy("CrmApi", policy =>
    {
        if (builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        {
            allowedOrigins =
            [
                "http://localhost:3000",
                "http://127.0.0.1:3000",
                "http://localhost:5173",
                "http://127.0.0.1:5173",
            ];
        }

        foreach (var origin in allowedOrigins)
        {
            if (!SecuritySupport.IsValidOrigin(origin, allowHttpForLocalhostOnly: builder.Environment.IsDevelopment()))
            {
                throw new InvalidOperationException($"Cors:AllowedOrigins contains an invalid origin: {origin}");
            }
        }

        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, _) =>
    {
        var endpoint = context.HttpContext.Request.Path.Value;
        var tenant = context.HttpContext.User.FindFirst("tenant_id")?.Value ??
            context.HttpContext.Request.Headers["X-Tenant-Id"].FirstOrDefault();
        crmRateLimitCounter.Add(
            1,
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("policy", "crm-global"),
            new KeyValuePair<string, object?>("reason", "rejected"),
            new KeyValuePair<string, object?>("tenant_hash", HashForTag(tenant)),
            new KeyValuePair<string, object?>("environment", builder.Environment.EnvironmentName.ToLowerInvariant()));

        return ValueTask.CompletedTask;
    };
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var partitionKey =
            context.User.FindFirst("tenant_id")?.Value ??
            context.User.FindFirst("tenantId")?.Value ??
            context.User.Identity?.Name ??
            context.Connection.RemoteIpAddress?.ToString() ??
            "anonymous";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PermitLimit", 600),
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            });
    });
    options.AddPolicy("crm-public-capture", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            BuildPublicRateLimitPartitionKey(context, "capture"),
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue("RateLimiting:PublicCapture:PermitLimit", 20),
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:PublicCapture:WindowSeconds", 60))
            }));
    options.AddPolicy("crm-marketing-consent", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            BuildPublicRateLimitPartitionKey(context, "marketing-consent"),
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue("RateLimiting:MarketingConsent:PermitLimit", 30),
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:MarketingConsent:WindowSeconds", 300))
            }));
    options.AddPolicy("crm-webhook", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            BuildPublicRateLimitPartitionKey(context, "webhook"),
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = builder.Configuration.GetValue("RateLimiting:Webhooks:PermitLimit", 120),
                QueueLimit = 0,
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:Webhooks:WindowSeconds", 60))
            }));
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSection = builder.Configuration.GetSection("Authentication:Jwt");
        var issuer = jwtSection["Issuer"];
        var audience = jwtSection["Audience"];
        var metadataAddress = jwtSection["MetadataAddress"];
        var authority = jwtSection["Authority"];
        var requireConfiguredJwt = !builder.Environment.IsDevelopment();

        if (requireConfiguredJwt &&
            (string.IsNullOrWhiteSpace(issuer) ||
             string.IsNullOrWhiteSpace(audience) ||
             (string.IsNullOrWhiteSpace(metadataAddress) && string.IsNullOrWhiteSpace(authority))))
        {
            throw new InvalidOperationException("Authentication:Jwt issuer, audience and metadata address or authority must be configured outside Development.");
        }

        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.IncludeErrorDetails = builder.Environment.IsDevelopment();
        options.SaveToken = false;
        if (!string.IsNullOrWhiteSpace(authority))
        {
            options.Authority = authority;
        }

        if (!string.IsNullOrWhiteSpace(metadataAddress))
        {
            options.MetadataAddress = metadataAddress;
        }

        options.RefreshOnIssuerKeyNotFound = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            NameClaimType = "name",
            RoleClaimType = "role",
            ValidTypes = ["at+jwt", "JWT"],
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddCustomerManagementPolicies();
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DevelopmentCrmSchemaBootstrapper.EnsureSchemasAsync(app.Services, app.Logger);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseMiddleware<CrmExceptionHandlingMiddleware>();
app.UseMiddleware<CrmRequestContextMiddleware>();
app.UseCrmSecurityHeaders();
app.UseResponseCompression();
if (!app.Environment.IsDevelopment() || !app.Configuration.GetValue<bool>("LocalDevelopment:DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}
app.UseCors("CrmApi");
app.UseAuthentication();
app.UseRateLimiter();
app.UseMiddleware<ProductionFeatureGateMiddleware>();
app.UseMiddleware<RequireTenantContextMiddleware>();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization();

app.MapGet("/api/crm/modules", () => Results.Ok(CrmModuleCatalog.Modules))
    .RequireAuthorization();

app.MapHealthChecks("/health/live", HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("live")))
    .AllowAnonymous();

var readyHealthOptions = app.Environment.IsDevelopment()
    ? HealthResponseWriter.CreateDetailedOptions(registration => registration.Tags.Contains("ready"))
    : HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("ready"));
var startupHealthOptions = app.Environment.IsDevelopment()
    ? HealthResponseWriter.CreateDetailedOptions(registration => registration.Tags.Contains("startup"))
    : HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("startup"));

app.MapHealthChecks("/health/ready", readyHealthOptions)
    .AllowAnonymous();

app.MapHealthChecks("/health/startup", startupHealthOptions)
    .AllowAnonymous();

var metricsEndpoint = app.MapPrometheusScrapingEndpoint("/metrics");
if (app.Environment.IsDevelopment())
{
    metricsEndpoint.AllowAnonymous();
}
else
{
    metricsEndpoint.RequireAuthorization();
}

await app.RunAsync();

static string BuildPublicRateLimitPartitionKey(HttpContext context, string policy)
{
    var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown-ip";
    var tenant = context.Request.RouteValues.TryGetValue("tenantId", out var tenantValue)
        ? tenantValue?.ToString()
        : context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
    return string.Join(':', policy, tenant ?? "anonymous", ip);
}

static string HashForTag(string? value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return "anonymous";
    }

    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
    return Convert.ToHexString(bytes[..6]).ToLowerInvariant();
}
