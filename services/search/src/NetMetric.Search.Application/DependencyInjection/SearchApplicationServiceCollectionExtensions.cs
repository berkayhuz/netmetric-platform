// <copyright file="SearchApplicationServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using NetMetric.Search.Application.Abstractions;
using NetMetric.Search.Application.StaticDocuments;
using NetMetric.Search.Application.IntegrationEvents;

namespace NetMetric.Search.Application.DependencyInjection;

public static class SearchApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddSearchApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SearchApplicationServiceCollectionExtensions).Assembly));
        services.AddSingleton<ISearchStaticTextLocalizer>(_ => SearchStaticTextLocalizer.CreateDefault());
        services.AddSingleton<StaticSearchDocumentFactory>();
        services.AddSingleton<IStaticSearchDocumentRegistry, StaticSearchDocumentRegistry>();
        services.AddScoped<ISearchIntegrationEventIngestionService, SearchIntegrationEventIngestionService>();
        return services;
    }
}
