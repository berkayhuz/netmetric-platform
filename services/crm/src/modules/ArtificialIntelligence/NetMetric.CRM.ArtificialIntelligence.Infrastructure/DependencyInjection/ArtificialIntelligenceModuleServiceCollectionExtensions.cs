// <copyright file="ArtificialIntelligenceModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.ArtificialIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.ArtificialIntelligence.Application.Commands.Providers.UpsertAiProvider;
using NetMetric.CRM.ArtificialIntelligence.Infrastructure.Health;
using NetMetric.CRM.ArtificialIntelligence.Infrastructure.Persistence;

namespace NetMetric.CRM.ArtificialIntelligence.Infrastructure.DependencyInjection;

public static class ArtificialIntelligenceModuleServiceCollectionExtensions
{
    public static IServiceCollection AddArtificialIntelligenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ArtificialIntelligenceConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ArtificialIntelligenceConnection connection string not found.");

        services.AddDbContext<ArtificialIntelligenceDbContext>((_, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ArtificialIntelligenceDbContext).Assembly.FullName));
        });

        services.AddScoped<IArtificialIntelligenceDbContext>(sp => sp.GetRequiredService<ArtificialIntelligenceDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IArtificialIntelligenceDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<UpsertAiProviderCommandValidator>();
        services.AddHealthChecks().AddCheck<ArtificialIntelligenceDbContextHealthCheck>("artificial-intelligence-db", HealthStatus.Unhealthy, ["ready", "db", "ai"]);

        return services;
    }
}
