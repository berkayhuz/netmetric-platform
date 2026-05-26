// <copyright file="ApplicationDependencyInjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using NetMetric.Notification.Application.Abstractions;
using NetMetric.Notification.Application.Options;
using NetMetric.Notification.Application.Services;

namespace NetMetric.Notification.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddNotificationApplication(this IServiceCollection services)
    {
        services
            .AddOptions<NotificationDispatchOptions>()
            .BindConfiguration(NotificationDispatchOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<INotificationProcessor, NotificationProcessor>();
        services.AddScoped<INotificationTemplateRenderer, SimpleNotificationTemplateRenderer>();
        services.AddScoped<INotificationChannelPolicy, DefaultNotificationChannelPolicy>();
        services.AddScoped<IUserNotificationPreferenceReader, AllowAllNotificationPreferenceReader>();
        services.AddSingleton<NotificationMetrics>();
        return services;
    }
}
