// <copyright file="CrmModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NetMetric.AspNetCore.CurrentUser;
using NetMetric.Authorization;
using NetMetric.Authorization.AspNetCore;
using NetMetric.Clock;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.DependencyInjection;
using NetMetric.CRM.API.Configuration;
using NetMetric.CRM.API.Features.Activities;
using NetMetric.CRM.ArtificialIntelligence.Infrastructure.DependencyInjection;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.CalendarSync.Infrastructure.DependencyInjection;
using NetMetric.CRM.ContractLifecycle.Infrastructure.DependencyInjection;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.DependencyInjection;
using NetMetric.CRM.CustomerManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.DealManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.DocumentManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.FinanceOperations.Infrastructure.DependencyInjection;
using NetMetric.CRM.IntegrationHub.Infrastructure.DependencyInjection;
using NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.LeadManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.MarketingAutomation.Infrastructure.DependencyInjection;
using NetMetric.CRM.Omnichannel.Infrastructure.DependencyInjection;
using NetMetric.CRM.OpportunityManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.PipelineManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.ProductCatalog.Infrastructure.DependencyInjection;
using NetMetric.CRM.QuoteManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.SalesForecasting.Infrastructure.DependencyInjection;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.DependencyInjection;
using NetMetric.CRM.TagManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.TenantManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.TicketManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.TicketSlaManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.TicketWorkflowManagement.Infrastructure.DependencyInjection;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.DependencyInjection;
using NetMetric.CRM.WorkManagement.Infrastructure.DependencyInjection;
using NetMetric.CurrentUser;
using NetMetric.Idempotency;
using NetMetric.Idempotency.DistributedCache;
using NetMetric.Idempotency.Redis;
using NetMetric.Media.Storage;
using NetMetric.MediatR;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;
using StackExchange.Redis;

namespace NetMetric.CRM.API.DependencyInjection;

public static class CrmModuleServiceCollectionExtensions
{
    public static IServiceCollection AddNetMetricCrm(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<HttpCurrentUserService>();
        services.TryAddScoped<ICurrentUserService>(sp => sp.GetRequiredService<HttpCurrentUserService>());
        services.TryAddScoped<ITenantContext>(sp => sp.GetRequiredService<HttpCurrentUserService>());
        services.TryAddScoped<ITenantProvider>(sp => sp.GetRequiredService<HttpCurrentUserService>());
        services.TryAddScoped<ICurrentAuthorizationScope, DefaultCurrentAuthorizationScope>();
        services.TryAddScoped<IFieldAuthorizationService, DefaultFieldAuthorizationService>();
        services.TryAddScoped<TenantIsolationSaveChangesInterceptor>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        services.TryAddScoped<SoftDeleteSaveChangesInterceptor>();
        services.TryAddScoped<IActivityTimelineReadService, ActivityTimelineReadService>();
        services.TryAddScoped<IActivityTimelineWriteService, ActivityTimelineWriteService>();
        services.TryAddSingleton<IClock, SystemClock>();
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrWhiteSpace(redisConnectionString))
        {
            services.TryAddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConnectionString));
            services.TryAddScoped<IIdempotencyStateStore, RedisIdempotencyStateStore>();
        }
        else
        {
            services.TryAddScoped<IIdempotencyStateStore, DistributedCacheIdempotencyStateStore>();
        }

        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionAuthorizationHandler>());
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>)));
        services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IPipelineBehavior<,>), typeof(RequestLoggingBehavior<,>)));

        services.AddHealthChecks().AddCheck("crm-api", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"]);

        services
            .AddCustomerManagementModule(configuration)
            .AddTenantManagementModule(configuration)
            .AddAnalyticsReportingModule(configuration)
            .AddArtificialIntelligenceModule(configuration)
            .AddCalendarSyncModule(configuration)
            .AddContractLifecycleModule(configuration)
            .AddCustomerIntelligenceModule(configuration)
            .AddDealManagementModule(configuration)
            .AddDocumentManagementModule(configuration)
            .AddFinanceOperationsModule(configuration)
            .AddIntegrationHubModule(configuration)
            .AddKnowledgeBaseManagementModule(configuration)
            .AddLeadManagementModule(configuration)
            .AddMarketingAutomationModule(configuration)
            .AddOmnichannelModule(configuration)
            .AddOpportunityManagementModule(configuration)
            .AddPipelineManagementModule(configuration)
            .AddProductCatalogModule(configuration)
            .AddQuoteManagementModule(configuration)
            .AddSalesForecastingModule(configuration)
            .AddSupportInboxIntegrationModule(configuration)
            .AddTagManagementModule(configuration)
            .AddTicketManagementModule(configuration)
            .AddTicketSlaManagementModule(configuration)
            .AddTicketWorkflowManagementModule(configuration)
            .AddWorkflowAutomationModule(configuration)
            .AddWorkManagementModule(configuration);

        services.AddSingleton(CrmModuleCatalog.Modules);
        services.AddNetMetricMedia(configuration, environment);

        return services;
    }
}
