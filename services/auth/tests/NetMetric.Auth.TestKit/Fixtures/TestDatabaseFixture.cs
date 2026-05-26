// <copyright file="TestDatabaseFixture.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.Auth.Infrastructure.Persistence;

namespace NetMetric.Auth.TestKit.Fixtures;

public sealed class TestDatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:;Cache=Shared");

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
    }

    public AuthDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseSqlite(_connection)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .Options;
        return new AuthDbContext(options);
    }

    public async Task ResetAsync()
    {
        await using var context = CreateDbContext();
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}

