// <copyright file="Program.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Search.Application.DependencyInjection;
using NetMetric.Search.Infrastructure.DependencyInjection;
using NetMetric.Search.Worker;
using NetMetric.Search.Worker.Integration;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services
    .AddOptions<SearchIntegrationConsumerOptions>()
    .BindConfiguration(SearchIntegrationConsumerOptions.SectionName)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSearchApplication();
builder.Services.AddSearchInfrastructure(builder.Configuration);
builder.Services.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddScoped<SearchIntegrationEventMessageDispatcher>();
builder.Services.AddHostedService<SearchIntegrationEventConsumer>();

await builder.Build().RunAsync();
