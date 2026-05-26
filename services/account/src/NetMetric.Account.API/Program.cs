// <copyright file="Program.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using NetMetric.Account.Api.DependencyInjection;
using NetMetric.Account.Api.Health;
using NetMetric.Account.Api.Http;
using NetMetric.Account.Api.Middleware;
using NetMetric.Account.Api.Options;
using NetMetric.Account.Application.DependencyInjection;
using NetMetric.Account.Infrastructure.DependencyInjection;
using NetMetric.Account.Persistence.DependencyInjection;
using NetMetric.Account.Persistence.Options;
using NetMetric.AspNetCore.Health;
using NetMetric.AspNetCore.Localization.DependencyInjection;
using NetMetric.Media.Options;

if (args.Contains("--healthcheck", StringComparer.OrdinalIgnoreCase))
{
    return;
}

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

ValidateProductionHosts(builder.Configuration, builder.Environment);

builder.Services
    .AddOptions<AccountOpenApiOptions>()
    .BindConfiguration(AccountOpenApiOptions.SectionName)
    .Validate(options => !string.IsNullOrWhiteSpace(options.Title), "OpenApi title is required.")
    .Validate(options => !string.IsNullOrWhiteSpace(options.Version), "OpenApi version is required.")
    .ValidateOnStart();

builder.Services
    .AddOptions<AccountDatabaseOptions>()
    .BindConfiguration(AccountDatabaseOptions.SectionName)
    .Validate(options => options.CommandTimeoutSeconds > 0, "Database command timeout must be greater than zero.")
    .Validate(options => options.MaxRetryCount >= 0, "Database max retry count cannot be negative.")
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<AccountDatabaseOptions>, AccountDatabaseOptionsValidation>();
builder.Services.AddControllers();
builder.Services.AddNetMetricLocalization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAccountOperationalHardening(builder.Configuration, builder.Environment);
builder.Services.AddAccountSwagger(builder.Configuration);
builder.Services.AddAccountApiAuthorization(builder.Configuration);
builder.Services.AddAccountApplication();
builder.Services.AddAccountInfrastructure(builder.Configuration, builder.Environment);

var connectionString = builder.Configuration.GetConnectionString("AccountDb");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'AccountDb' is required.");
}

builder.Services.AddAccountPersistence(connectionString);
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Account API process is running."), tags: ["live"])
    .AddCheck<AccountDbHealthCheck>("account-db", tags: ["ready", "startup"])
    .AddCheck<AccountPendingMigrationsHealthCheck>("account-db-migrations", tags: ["ready", "startup"]);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment() || !app.Configuration.GetValue<bool>("LocalDevelopment:DisableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}
app.UseRouting();
app.UseNetMetricLocalization();
app.UseAccountOperationalHardening();
app.UseAccountExceptionHandling();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseDevelopmentLocalMedia();
app.UseAuthentication();
app.UseMiddleware<SessionActivityMiddleware>();
app.UseAuthorization();
app.MapControllers()
    .RequireRateLimiting(AccountOperationalHardeningExtensions.GlobalRateLimitPolicy);
app.MapHealthChecks(
        "/health/live",
        HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("live")))
    .DisableRateLimiting()
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/ready",
        HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("ready")))
    .DisableRateLimiting()
    .AllowAnonymous();
app.MapHealthChecks(
        "/health/startup",
        HealthResponseWriter.CreateMinimalOptions(registration => registration.Tags.Contains("startup")))
    .DisableRateLimiting()
    .AllowAnonymous();
app.MapPrometheusScrapingEndpoint("/metrics");

await app.ApplyAccountDatabaseMigrationsAsync();

await app.RunAsync();

static void ValidateProductionHosts(IConfiguration configuration, IHostEnvironment environment)
{
    if (!environment.IsProduction())
    {
        return;
    }

    var allowedHosts = configuration["AllowedHosts"];
    var hosts = allowedHosts?.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries) ?? [];
    if (string.IsNullOrWhiteSpace(allowedHosts) ||
        hosts.Contains("*", StringComparer.Ordinal) ||
        hosts.Any(host =>
            host.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
            host.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
            host.Contains("example", StringComparison.OrdinalIgnoreCase) ||
            host.Contains("LOCAL", StringComparison.OrdinalIgnoreCase) ||
            host.Contains("DEV", StringComparison.OrdinalIgnoreCase) ||
            host.Contains("TEST", StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException("AllowedHosts must name production Account API hosts outside Development.");
    }
}

internal static class LocalMediaApplicationBuilderExtensions
{
    public static IApplicationBuilder UseDevelopmentLocalMedia(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment() &&
            !app.Environment.IsEnvironment("Test") &&
            !app.Environment.IsEnvironment("Testing"))
        {
            return app;
        }

        var mediaOptions = app.Services.GetRequiredService<IOptions<MediaOptions>>().Value;
        if (!string.Equals(mediaOptions.StorageProvider, "LocalFile", StringComparison.OrdinalIgnoreCase))
        {
            return app;
        }

        var rootPath = Path.GetFullPath(mediaOptions.Local.RootPath);
        Directory.CreateDirectory(rootPath);

        return app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(rootPath),
            RequestPath = mediaOptions.Local.RequestPath,
            ServeUnknownFileTypes = false,
            OnPrepareResponse = context =>
            {
                context.Context.Response.Headers.CacheControl = "public, max-age=300";
            }
        });
    }
}

public partial class Program;
