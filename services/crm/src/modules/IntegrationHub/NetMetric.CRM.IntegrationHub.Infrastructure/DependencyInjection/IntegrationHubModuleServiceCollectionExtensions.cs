// <copyright file="IntegrationHubModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Processing;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Providers;
using NetMetric.CRM.IntegrationHub.Application.Commands.UpsertIntegrationConnection;
using NetMetric.CRM.IntegrationHub.Infrastructure.Connectors;
using NetMetric.CRM.IntegrationHub.Infrastructure.Health;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;
using NetMetric.CRM.IntegrationHub.Infrastructure.Processing;
using NetMetric.CRM.IntegrationHub.Infrastructure.Providers;
using NetMetric.CRM.IntegrationHub.Infrastructure.Webhooks;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.DependencyInjection;

public static class IntegrationHubModuleServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationHubModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("IntegrationHubConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("IntegrationHubConnection connection string not found.");

        services.AddDbContext<IntegrationHubDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(IntegrationHubDbContext).Assembly.FullName)));

        services.AddScoped<IIntegrationHubDbContext>(sp => sp.GetRequiredService<IntegrationHubDbContext>());
        services.Configure<IntegrationJobProcessingOptions>(configuration.GetSection("Crm:IntegrationHub:JobProcessing"));
        services.PostConfigure<IntegrationJobProcessingOptions>(options =>
        {
            options.Enabled = configuration.GetValue<bool>("Crm:Features:IntegrationJobProcessingEnabled");
        });
        services.AddSingleton<IIntegrationJobProcessingState, IntegrationJobProcessingState>();
        services.AddScoped<IIntegrationJobProcessor, IntegrationJobProcessor>();
        services.AddSingleton<IIntegrationConnectorRegistry, ServiceCollectionIntegrationConnectorRegistry>();
        services.AddSingleton<IIntegrationWebhookSecurityService, HmacIntegrationWebhookSecurityService>();
        services
            .AddOptions<IntegrationProviderCatalogOptions>()
            .Bind(configuration.GetSection(IntegrationProviderCatalogOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<IntegrationProviderCatalogOptions>, IntegrationProviderCatalogOptionsValidation>();
        services.AddSingleton<IIntegrationProviderCatalog, DefaultIntegrationProviderCatalog>();
        services.Configure<WhatsAppProviderOptions>(configuration.GetSection(WhatsAppProviderOptions.SectionName));
        services.AddScoped<WhatsAppCloudAdapter>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<WhatsAppProviderOptions>>();
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(Math.Max(1, options.Value.TimeoutSeconds))
            };
            return new WhatsAppCloudAdapter(client, options);
        });
        services.AddScoped<IProviderCredentialValidator, DefaultProviderCredentialValidator>();
        services.AddScoped<IProviderConnectionTester, DefaultProviderConnectionTester>();
        services.AddScoped<IProviderAdapter, MockProviderAdapter>();
        services.AddScoped<IProviderAdapter>(sp => sp.GetRequiredService<WhatsAppCloudAdapter>());
        services.AddScoped<IProviderAdapter, InstagramMessagingAdapter>();
        services.AddScoped<IProviderAdapterRegistry, ProviderAdapterRegistry>();
        services.AddHostedService<IntegrationJobBackgroundService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(UpsertIntegrationConnectionCommandHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(UpsertIntegrationConnectionCommandHandler).Assembly);

        services.AddHealthChecks()
            .AddCheck<IntegrationHubDbContextHealthCheck>(
                "integration-hub-db",
                HealthStatus.Unhealthy,
                ["db", "integration-hub", "ready"])
            .AddCheck<IntegrationJobWorkerHealthCheck>(
                "integration-hub-worker",
                HealthStatus.Degraded,
                ["worker", "integration-hub", "ready"]);

        return services;
    }
}
