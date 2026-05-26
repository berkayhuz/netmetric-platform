// <copyright file="TenantIsolationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.ContractLifecycle.Domain.Entities.Contracts;
using NetMetric.CRM.ContractLifecycle.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.ContractLifecycle.ArchitectureTests;

public sealed class TenantIsolationTests
{
    [Fact]
    public async Task ContractLifecycleDbContext_Should_Filter_By_Current_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        await using (var seedContext = CreateContext(connection, null))
        {
            await seedContext.Database.EnsureCreatedAsync();

            var contractA = new ContractRecord("A-001", "Tenant A");
            contractA.TenantId = tenantA;

            var contractB = new ContractRecord("B-001", "Tenant B");
            contractB.TenantId = tenantB;

            await seedContext.Contracts.AddRangeAsync(contractA, contractB);
            await seedContext.SaveChangesAsync();
        }

        await using (var tenantAContext = CreateContext(connection, tenantA))
        {
            var visibleIds = await tenantAContext.Contracts.AsNoTracking().Select(x => x.TenantId).Distinct().ToListAsync();
            visibleIds.Should().Equal(tenantA);
        }

        await using var tenantlessContext = CreateContext(connection, null);
        var contracts = await tenantlessContext.Contracts.AsNoTracking().ToListAsync();
        contracts.Should().BeEmpty();
    }

    private static ContractLifecycleDbContext CreateContext(SqliteConnection connection, Guid? tenantId)
    {
        var options = new DbContextOptionsBuilder<ContractLifecycleDbContext>()
            .UseSqlite(connection)
            .Options;

        var tenantContext = new FakeTenantContext(tenantId);
        var currentUserService = new FakeCurrentUserService(tenantId);
        return new ContractLifecycleDbContext(
            options,
            tenantContext,
            new TenantIsolationSaveChangesInterceptor(tenantContext),
            new AuditSaveChangesInterceptor(currentUserService),
            new SoftDeleteSaveChangesInterceptor(currentUserService));
    }

    private sealed class FakeTenantContext : ITenantContext
    {
        public FakeTenantContext(Guid? tenantId)
        {
            TenantId = tenantId;
        }

        public Guid? TenantId { get; }
        public bool IsAuthenticated => TenantId.HasValue;

        public void EnsureAuthenticated()
        {
            if (!IsAuthenticated)
                throw new ForbiddenAppException("Authentication is required.");
        }

        public Guid GetRequiredTenantId()
        {
            EnsureAuthenticated();
            return TenantId!.Value;
        }
    }

    private sealed class FakeCurrentUserService : ICurrentUserService
    {
        private readonly Guid? _tenantId;

        public FakeCurrentUserService(Guid? tenantId)
        {
            _tenantId = tenantId;
        }

        public bool IsAuthenticated => _tenantId.HasValue;
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId => _tenantId ?? throw new ForbiddenAppException("Tenant context is missing.");
        public string? UserName => "test-user";
        public string? Email => "test@example.com";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;

        public void EnsureAuthenticated()
        {
            if (!IsAuthenticated)
                throw new ForbiddenAppException("Authentication is required.");
        }

        public void EnsureTenantAccess(Guid requestedTenantId)
        {
            if (TenantId != requestedTenantId)
                throw new ForbiddenAppException("Tenant access denied.");
        }
    }
}
