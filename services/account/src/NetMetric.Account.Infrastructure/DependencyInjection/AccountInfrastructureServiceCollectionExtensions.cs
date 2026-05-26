// <copyright file="AccountInfrastructureServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetMetric.Account.Application.Abstractions.Audit;
using NetMetric.Account.Application.Abstractions.Identity;
using NetMetric.Account.Application.Abstractions.Membership;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Infrastructure.Audit;
using NetMetric.Account.Infrastructure.Identity;
using NetMetric.Account.Infrastructure.IntegrationEvents;
using NetMetric.Account.Infrastructure.Media;
using NetMetric.Account.Infrastructure.Membership;
using NetMetric.Account.Infrastructure.Outbox;
using NetMetric.Account.Infrastructure.Security;
using NetMetric.AspNetCore.TrustedGateway.Options;
using NetMetric.Clock;
using NetMetric.Media.Storage;
using NetMetric.Messaging.RabbitMq.DependencyInjection;

namespace NetMetric.Account.Infrastructure.DependencyInjection;

public static class AccountInfrastructureServiceCollectionExtensions
{
    private static IServiceCollection AddAccountInfrastructureCore(this IServiceCollection services)
    {
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IAccountAuditWriter, PersistentAccountAuditWriter>();
        services.AddScoped<ISecurityEventWriter, PersistentSecurityEventWriter>();
        services.AddScoped<ISecurityNotificationPublisher, OutboxSecurityNotificationPublisher>();
        services.AddScoped<NetMetric.Account.Application.Abstractions.Outbox.IAccountOutboxWriter, AccountOutboxWriter>();
        services.AddSingleton<IAuditMetadataSanitizer, AuditMetadataSanitizer>();

        return services;
    }

    public static IServiceCollection AddAccountInfrastructure(this IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment environment)
    {
        services
            .AddOptions<AccountAuditOptions>()
            .Bind(configuration.GetSection(AccountAuditOptions.SectionName))
            .Validate(options => options.MetadataMaxLength > 0, "Audit metadata max length must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<IdentityServiceOptions>()
            .Bind(configuration.GetSection(IdentityServiceOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<IdentityServiceOptions>, IdentityServiceOptionsValidation>();

        services
            .AddOptions<TrustedGatewayOptions>()
            .Bind(configuration.GetSection(TrustedGatewayOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<TrustedGatewayOptions>, AccountTrustedGatewayOptionsValidation>();

        services
            .AddOptions<MembershipServiceOptions>()
            .Bind(configuration.GetSection(MembershipServiceOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<MembershipServiceOptions>, MembershipServiceOptionsValidation>();

        services
            .AddOptions<AccountOutboxOptions>()
            .Bind(configuration.GetSection(AccountOutboxOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountOutboxOptions>, AccountOutboxOptionsValidation>();

        services
            .AddOptions<AccountMediaCleanupOptions>()
            .Bind(configuration.GetSection(AccountMediaCleanupOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AccountMediaCleanupOptions>, AccountMediaCleanupOptionsValidation>();

        services
            .AddOptions<AuthProfileBootstrapOptions>()
            .Bind(configuration.GetSection(AuthProfileBootstrapOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<AuthProfileBootstrapOptions>, AuthProfileBootstrapOptionsValidation>();
        services.AddNetMetricMedia(configuration, environment);

        services.AddAccountInfrastructureCore();
        services.AddRabbitMqMessaging(configuration);
        services.AddScoped<IAccountIntegrationEventPublisher, RabbitMqAccountIntegrationEventPublisher>();
        services.AddScoped<AccountMediaCleanupService>();
        services.AddHostedService<AccountOutboxProcessor>();
        services.AddHostedService<AccountMediaCleanupWorker>();
        services.AddHostedService<AuthUserRegisteredProfileBootstrapConsumer>();
        services.AddTransient<IdentityInternalRequestSigningHandler>();
        services.AddTransient<IdentityResilienceHandler>();

        services.AddHttpClient<IIdentityAccountClient, HttpIdentityAccountClient>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<IdentityServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        })
            .AddHttpMessageHandler<IdentityResilienceHandler>()
            .AddHttpMessageHandler<IdentityInternalRequestSigningHandler>();

        services.AddHttpClient<IMembershipReadService, HttpMembershipReadService>((provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<MembershipServiceOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        }).AddStandardResilienceHandler();

        return services;
    }
}
