// <copyright file="CalendarSyncModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.CalendarSync.Application.Abstractions.Persistence;
using NetMetric.CRM.CalendarSync.Application.Commands.Connections.UpsertCalendarConnection;
using NetMetric.CRM.CalendarSync.Infrastructure.Health;
using NetMetric.CRM.CalendarSync.Infrastructure.Persistence;

namespace NetMetric.CRM.CalendarSync.Infrastructure.DependencyInjection;

public static class CalendarSyncModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCalendarSyncModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CalendarSyncConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("CalendarSyncConnection connection string not found.");

        services.AddDbContext<CalendarSyncDbContext>((_, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(CalendarSyncDbContext).Assembly.FullName));
        });

        services.AddScoped<ICalendarSyncDbContext>(sp => sp.GetRequiredService<CalendarSyncDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ICalendarSyncDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<UpsertCalendarConnectionCommandValidator>();
        services.AddHealthChecks().AddCheck<CalendarSyncDbContextHealthCheck>("calendar-sync-db", HealthStatus.Unhealthy, ["ready", "db", "calendar"]);

        return services;
    }
}
