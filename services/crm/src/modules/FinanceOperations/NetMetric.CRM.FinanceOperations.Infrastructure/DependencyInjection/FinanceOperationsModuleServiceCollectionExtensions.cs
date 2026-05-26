// <copyright file="FinanceOperationsModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.FinanceOperations.Application.Abstractions.Persistence;
using NetMetric.CRM.FinanceOperations.Application.Features.Orders.Commands.CreateSalesOrder;
using NetMetric.CRM.FinanceOperations.Infrastructure.Health;
using NetMetric.CRM.FinanceOperations.Infrastructure.Persistence;

namespace NetMetric.CRM.FinanceOperations.Infrastructure.DependencyInjection;

public static class FinanceOperationsModuleServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceOperationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FinanceOperationsConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("FinanceOperationsConnection connection string not found.");

        services.AddDbContext<FinanceOperationsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(FinanceOperationsDbContext).Assembly.FullName));
        });

        services.AddScoped<IFinanceOperationsDbContext>(sp => sp.GetRequiredService<FinanceOperationsDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateSalesOrderCommandHandler).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateSalesOrderCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<FinanceOperationsDbContextHealthCheck>(
                name: "finance-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "finance"]);

        return services;
    }
}
