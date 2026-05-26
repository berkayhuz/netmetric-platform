// <copyright file="ContractLifecycleModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.ContractLifecycle.Application.Abstractions.Persistence;
using NetMetric.CRM.ContractLifecycle.Application.Features.Contracts.Commands.CreateContractRecord;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Health;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence;

namespace NetMetric.CRM.ContractLifecycle.Infrastructure.DependencyInjection;

public static class ContractLifecycleModuleServiceCollectionExtensions
{
    public static IServiceCollection AddContractLifecycleModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ContractLifecycleConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ContractLifecycleConnection connection string not found.");

        services.AddDbContext<ContractLifecycleDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(ContractLifecycleDbContext).Assembly.FullName));
        });

        services.AddScoped<IContractLifecycleDbContext>(sp => sp.GetRequiredService<ContractLifecycleDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateContractRecordCommandHandler).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateContractRecordCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<ContractLifecycleDbContextHealthCheck>(
                name: "contracts-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "contracts"]);

        return services;
    }
}
