// <copyright file="NotificationDbContextFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NetMetric.Notification.Infrastructure.Persistence.DesignTime;

public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("NETMETRIC_NOTIFICATION_MIGRATIONS_CONNECTION")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__NotificationConnection")
            ?? "Server=localhost;Database=NetMetric_Notification;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName))
            .Options;

        return new NotificationDbContext(options);
    }
}
