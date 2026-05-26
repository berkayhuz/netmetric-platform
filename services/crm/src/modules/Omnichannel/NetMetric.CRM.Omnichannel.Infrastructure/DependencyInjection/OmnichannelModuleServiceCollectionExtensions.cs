// <copyright file="OmnichannelModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Application.Commands.Accounts.CreateChannelAccount;
using NetMetric.CRM.Omnichannel.Infrastructure.Health;
using NetMetric.CRM.Omnichannel.Infrastructure.Persistence;

namespace NetMetric.CRM.Omnichannel.Infrastructure.DependencyInjection;

public static class OmnichannelModuleServiceCollectionExtensions
{
    public static IServiceCollection AddOmnichannelModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OmnichannelConnection")
            ?? throw new InvalidOperationException("OmnichannelConnection connection string not found.");

        services.AddDbContext<OmnichannelDbContext>((_, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(OmnichannelDbContext).Assembly.FullName));
        });

        services.AddScoped<IOmnichannelDbContext>(sp => sp.GetRequiredService<OmnichannelDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IOmnichannelDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateChannelAccountCommandValidator>();
        services.AddHealthChecks().AddCheck<OmnichannelDbContextHealthCheck>("omnichannel-db", HealthStatus.Unhealthy, ["ready", "db", "omnichannel"]);

        return services;
    }
}
