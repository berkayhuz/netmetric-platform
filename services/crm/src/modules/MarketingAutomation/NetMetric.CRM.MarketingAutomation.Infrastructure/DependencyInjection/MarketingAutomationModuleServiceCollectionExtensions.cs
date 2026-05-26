// <copyright file="MarketingAutomationModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.MarketingAutomation.Application.Features.Campaigns.Commands.CreateCampaign;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Health;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Security;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.DependencyInjection;

public static class MarketingAutomationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddMarketingAutomationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MarketingAutomationConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("MarketingAutomationConnection connection string not found.");

        services.AddDbContext<MarketingAutomationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(MarketingAutomationDbContext).Assembly.FullName));
        });

        services.AddScoped<IMarketingAutomationDbContext>(sp => sp.GetRequiredService<MarketingAutomationDbContext>());
        services.Configure<MarketingAutomationOptions>(configuration.GetSection(MarketingAutomationOptions.SectionName));
        services.Configure<MarketingConsentTokenOptions>(configuration.GetSection(MarketingConsentTokenOptions.SectionName));
        services.PostConfigure<MarketingAutomationOptions>(options =>
        {
            options.EngineEnabled = configuration.GetValue("Crm:Features:MarketingAutomationEngineEnabled", options.EngineEnabled);
            options.WorkerEnabled = configuration.GetValue("Crm:Features:MarketingAutomationWorkerEnabled", options.WorkerEnabled);
            options.EmailDeliveryEnabled = configuration.GetValue("Crm:Features:MarketingEmailDeliveryEnabled", options.EmailDeliveryEnabled);
        });
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IMarketingAutomationService, MarketingAutomationService>();
        services.AddScoped<IMarketingConsentTokenService, MarketingConsentTokenService>();
        services.AddScoped<IMarketingSegmentEvaluator, MarketingSegmentEvaluator>();
        services.AddScoped<IMarketingConsentEnforcementService, MarketingConsentEnforcementService>();
        services.AddScoped<IMarketingTemplateRenderer, MarketingTemplateRenderer>();
        services.AddScoped<IMarketingCampaignScheduler, MarketingCampaignScheduler>();
        services.AddScoped<IMarketingJourneyExecutor, MarketingJourneyExecutor>();
        services.AddScoped<IMarketingPermissionGuard, MarketingPermissionGuard>();
        services.AddScoped<IMarketingEmailDeliveryProvider, DisabledMarketingEmailDeliveryProvider>();
        services.AddHostedService<MarketingAutomationBackgroundService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateCampaignCommandHandler).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateCampaignCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<MarketingAutomationDbContextHealthCheck>(
                name: "marketing-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "marketing"]);

        return services;
    }
}
