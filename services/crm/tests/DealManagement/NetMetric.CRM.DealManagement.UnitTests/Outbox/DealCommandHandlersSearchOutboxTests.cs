// <copyright file="DealCommandHandlersSearchOutboxTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.DealManagement.Application.Handlers;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DealManagement.UnitTests.Outbox;

public sealed class DealCommandHandlersSearchOutboxTests
{
    [Fact]
    public async Task CreateDealCommandHandler_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new CapturingOutbox();
        var sut = new CreateDealCommandHandler(fixture.DbContext, fixture.CurrentUser, outbox);

        var result = await sut.Handle(
            new CreateDealCommand("DEAL-2026-3001", "Search create deal", 250m, DateTime.UtcNow, null, null, null, "private notes"),
            CancellationToken.None);

        result.Name.Should().Be("Search create deal");
        outbox.CreatedDealIds.Should().ContainSingle(x => x == result.Id);
    }

    [Fact]
    public async Task UpdateDealCommandHandler_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedDealAsync();
        var outbox = new CapturingOutbox();
        var sut = new UpdateDealCommandHandler(fixture.DbContext, fixture.CurrentUser, outbox);

        await sut.Handle(
            new UpdateDealCommand(
                seeded.Id,
                "DEAL-2026-3002",
                "Search update deal",
                500m,
                DateTime.UtcNow,
                null,
                null,
                null,
                "private update note",
                Convert.ToBase64String(seeded.RowVersion)),
            CancellationToken.None);

        outbox.UpdatedDealIds.Should().ContainSingle(x => x == seeded.Id);
    }

    [Fact]
    public async Task SoftDeleteDealCommandHandler_Should_Enqueue_Search_Delete_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedDealAsync();
        var outbox = new CapturingOutbox();
        var sut = new SoftDeleteDealCommandHandler(fixture.DbContext, fixture.CurrentUser, outbox);

        await sut.Handle(new SoftDeleteDealCommand(seeded.Id), CancellationToken.None);

        outbox.DeletedDealIds.Should().ContainSingle(x => x == seeded.Id);
    }

    private sealed class CapturingOutbox : IDealManagementOutbox
    {
        public List<Guid> CreatedDealIds { get; } = [];
        public List<Guid> UpdatedDealIds { get; } = [];
        public List<Guid> DeletedDealIds { get; } = [];

        public Task EnqueueDealCreatedAsync(Deal deal, CancellationToken cancellationToken)
        {
            CreatedDealIds.Add(deal.Id);
            return Task.CompletedTask;
        }

        public Task EnqueueDealCreatedAndPersistAsync(Deal deal, CancellationToken cancellationToken)
        {
            CreatedDealIds.Add(deal.Id);
            return Task.CompletedTask;
        }

        public Task EnqueueDealUpdatedAsync(Deal deal, CancellationToken cancellationToken)
        {
            UpdatedDealIds.Add(deal.Id);
            return Task.CompletedTask;
        }

        public Task EnqueueDealDeletedAsync(Deal deal, CancellationToken cancellationToken)
        {
            DeletedDealIds.Add(deal.Id);
            return Task.CompletedTask;
        }

        public Task EnqueueDealRestoredAsync(Deal deal, CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task EnqueueDealPurgedAsync(Guid tenantId, Guid dealId, string? dealName, Guid? ownerUserId, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public DealManagementDbContext DbContext { get; }
        public ICurrentUserService CurrentUser { get; }

        private Fixture(SqliteConnection connection, DealManagementDbContext dbContext, ICurrentUserService currentUser)
        {
            this.connection = connection;
            DbContext = dbContext;
            CurrentUser = currentUser;
        }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<DealManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new DealManagementDbContext(
                options,
                new FixedTenantProvider(tenantId),
                new TenantIsolationSaveChangesInterceptor(),
                new AuditSaveChangesInterceptor(),
                new SoftDeleteSaveChangesInterceptor());

            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(connection, dbContext, new FixedCurrentUser(tenantId));
        }

        public async Task<Deal> SeedDealAsync()
        {
            var entity = new Deal
            {
                TenantId = CurrentUser.TenantId,
                DealCode = "DEAL-2026-3999",
                Name = "Seed Deal",
                TotalAmount = 100m,
                ClosedDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                UpdatedBy = "tester"
            };

            DbContext.Deals.Add(entity);
            await DbContext.SaveChangesAsync();
            return entity;
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? TenantId => tenantId;
    }

    private sealed class FixedCurrentUser(Guid tenantId) : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => true;
        public string? UserName => "tester";
        public string? Email => "tester@example.test";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
    }
}
