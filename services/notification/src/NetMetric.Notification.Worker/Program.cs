// <copyright file="Program.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.Notification.Application.DependencyInjection;
using NetMetric.Notification.Application.Services;
using NetMetric.Notification.Infrastructure.DependencyInjection;
using NetMetric.Notification.Worker;
using NetMetric.Notification.Worker.Health;
using NetMetric.Notification.Worker.Workers;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<NotificationWorkerOptions>()
    .BindConfiguration(NotificationWorkerOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddNotificationApplication();
builder.Services.AddNotificationInfrastructure(builder.Configuration);
builder.Services.AddHostedService<NotificationQueueConsumerService>();
builder.Services.AddSingleton<NotificationWorkerMetrics>();
builder.Services.AddHealthChecks()
    .AddCheck("notification-worker-live", () => HealthCheckResult.Healthy("Notification worker process is running."), tags: ["live"])
    .AddCheck<NotificationRabbitMqHealthCheck>("notification-rabbitmq", tags: ["ready", "notification"]);

AddOpenTelemetry(builder.Services, builder.Configuration, "NetMetric.Notification.Worker");

var host = builder.Build();
await host.Services.InitializeNotificationInfrastructureAsync(CancellationToken.None);
await host.RunAsync();

static void AddOpenTelemetry(IServiceCollection services, IConfiguration configuration, string serviceName)
{
    var otlpEndpoint = configuration["OpenTelemetry:Otlp:Endpoint"] ?? configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
    var telemetry = services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService(serviceName));

    telemetry.WithMetrics(metrics =>
    {
        metrics.AddMeter(NotificationMetrics.MeterName);
        metrics.AddMeter(NotificationWorkerMetrics.MeterName);

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    });

    telemetry.WithTracing(tracing =>
    {
        tracing.AddSource("NetMetric.Notification.Worker");

        if (!string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
        }
    });
}
