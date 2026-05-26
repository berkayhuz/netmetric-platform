// <copyright file="RabbitMqServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetMetric.Messaging.Abstractions;
using NetMetric.Messaging.RabbitMq.Connection;
using NetMetric.Messaging.RabbitMq.Options;
using NetMetric.Messaging.RabbitMq.Publishing;

namespace NetMetric.Messaging.RabbitMq.DependencyInjection;

public static class RabbitMqServiceCollectionExtensions
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidation>();
        services.AddSingleton<RabbitMqConnectionProvider>();
        services.AddSingleton<IIntegrationEventPublisher, RabbitMqIntegrationEventPublisher>();
        return services;
    }
}
