// <copyright file="CustomerManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.AspNetCore.CurrentUser;
using NetMetric.Authorization;
using NetMetric.Authorization.AspNetCore;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Integration;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Services;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Features.Imports.Commands.ImportCompanies;
using NetMetric.CRM.CustomerManagement.Infrastructure.Health;
using NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence.Interceptors;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Infrastructure.TrashRetention;
using NetMetric.CurrentUser;
using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Repository;
using NetMetric.Repository.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.DependencyInjection;

public static class CustomerManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerManagementModule(this IServiceCollection services, IConfiguration configuration)
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
        services.TryAddEnumerable(ServiceDescriptor.Scoped<IAuthorizationHandler, PermissionAuthorizationHandler>());

        services.AddOptions<CustomerManagementSecurityOptions>()
            .Bind(configuration.GetSection("Security:CustomerDataSecurity"));
        services.AddScoped<ICustomerManagementSecurityService, CustomerManagementSecurityService>();
        services.AddScoped<CustomerManagementFieldSecuritySaveChangesInterceptor>();
        services.AddScoped<CustomerManagementRowLevelSecuritySaveChangesInterceptor>();

        services.AddDbContext<CustomerManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("CustomerManagementConnection")
                ?? throw new InvalidOperationException("CustomerManagementConnection connection string not found.");

            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(CustomerManagementDbContext).Assembly.FullName));

            options.AddInterceptors(
                sp.GetRequiredService<TenantIsolationSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<CustomerManagementFieldSecuritySaveChangesInterceptor>(),
                sp.GetRequiredService<CustomerManagementRowLevelSecuritySaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>());
        });

        services.AddScoped<ICustomerManagementDbContext>(sp => sp.GetRequiredService<CustomerManagementDbContext>());

        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<IRepository<Address, Guid>>(sp =>
            new EfRepository<Address, Guid>(sp.GetRequiredService<CustomerManagementDbContext>()));
        services.AddScoped<IRepository<Company, Guid>>(sp =>
            new EfRepository<Company, Guid>(sp.GetRequiredService<CustomerManagementDbContext>()));
        services.AddScoped<IRepository<Contact, Guid>>(sp =>
            new EfRepository<Contact, Guid>(sp.GetRequiredService<CustomerManagementDbContext>()));
        services.AddScoped<IRepository<Customer, Guid>>(sp =>
            new EfRepository<Customer, Guid>(sp.GetRequiredService<CustomerManagementDbContext>()));

        services.AddScoped<ICustomerManagementMergeService, CustomerManagementMergeService>();
        services.AddScoped<ICustomerManagementOutbox, CustomerManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<CustomerManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(CustomerManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<CustomerManagementOutboxMetrics>();
        services.AddScoped<ICustomerManagementOutboxPublisher, RabbitMqCustomerManagementOutboxPublisher>();
        services.AddScoped<ICustomerManagementOutboxProcessor, CustomerManagementOutboxProcessor>();
        services.AddHostedService<CustomerManagementOutboxBackgroundService>();
        services.AddOptions<CustomerManagementTrashRetentionOptions>()
            .Bind(configuration.GetSection(CustomerManagementTrashRetentionOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<ICustomerManagementTrashRetentionProcessor, CustomerManagementTrashRetentionProcessor>();
        services.AddHostedService<CustomerManagementTrashRetentionBackgroundService>();
        services.AddScoped<ICustomerAdministrationService, CustomerAdministrationService>();
        services.AddScoped<IGlobalTrashIndexWriter, GlobalTrashIndexWriter>();
        services.AddScoped<IContactAdministrationService, ContactAdministrationService>();
        services.AddScoped<ICompanyAdministrationService, CompanyAdministrationService>();
        services.AddScoped<IAddressAdministrationService, AddressAdministrationService>();
        services.AddOptions<CustomerPortalOptions>()
            .Bind(configuration.GetSection(CustomerPortalOptions.SectionName));
        services.AddScoped<ICustomerLeadSummaryProvider, LeadManagementCustomerSummaryProvider>();
        services.AddScoped<ICustomerOpportunitySummaryProvider, OpportunityManagementCustomerSummaryProvider>();
        services.AddScoped<ICustomerDealSummaryProvider, DealManagementCustomerSummaryProvider>();
        services.AddScoped<ICustomerQuoteSummaryProvider, QuoteManagementCustomerSummaryProvider>();
        services.AddScoped<ICustomerTicketSummaryProvider, TicketManagementCustomerSummaryProvider>();
        services.AddScoped<ICustomerFinanceSummaryProvider, FinanceOperationsCustomerSummaryProvider>();
        services.AddScoped<ICustomerContractSummaryProvider, ContractLifecycleCustomerSummaryProvider>();
        services.AddScoped<ICustomerActivitySummaryProvider, WorkManagementCustomerActivitySummaryProvider>();
        services.AddScoped<ICustomerCommunicationSummaryProvider, SupportInboxCustomerCommunicationSummaryProvider>();
        services.AddScoped<ICustomerAccountHierarchyProvider, CustomerIntelligenceAccountHierarchyProvider>();
        services.AddScoped<ICustomerStakeholderMapProvider, CustomerManagementStakeholderMapProvider>();
        services.AddScoped<ICustomerDocumentSummaryProvider, CustomerManagementDocumentSummaryProvider>();
        services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
        services.AddScoped<ICustomerDataQualityService, CustomerDataQualityService>();
        services.AddScoped<ICustomerRelationshipHealthService, CustomerRelationshipHealthService>();
        services.AddScoped<ICustomerAuditTrailWriter, CustomerAuditTrailWriter>();
        services.AddScoped<ICustomerOwnershipRuleEvaluator, CustomerOwnershipRuleEvaluator>();
        services.AddScoped<ICustomerSearchIndexer, CustomerSearchIndexer>();
        services.AddScoped<ICustomerSearchService, CustomerSearchService>();
        services.AddScoped<ICustomerEnrichmentProvider, NoExternalCallCustomerEnrichmentProvider>();
        services.AddScoped<ICustomerEnrichmentService, CustomerEnrichmentService>();

        services.AddValidatorsFromAssemblyContaining<ImportCompaniesCommandValidator>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<ImportCompaniesCommand>();
            cfg.RegisterServicesFromAssemblyContaining<CustomerIntelligenceAccountHierarchyProvider>();
        });

        services.AddHealthChecks()
            .AddCheck<CustomerManagementDbContextHealthCheck>(
                name: "customer-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "customer-management"]);

        return services;
    }
}
