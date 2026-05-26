// <copyright file="GetGlobalTrashItemsQueryHandlerTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Queries.Trash;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Queries;

public sealed class GetGlobalTrashItemsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnEmptyPagedResult_WhenNoItemsExist()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await TrashQueryFixture.CreateAsync(tenantId);

        var handler = fixture.CreateHandler(tenantId, userId);
        var result = await handler.Handle(new GetGlobalTrashItemsQuery(Page: 1, PageSize: 20), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveItemsFromCurrentTenant()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await TrashQueryFixture.CreateAsync(tenantId);

        await fixture.SeedAsync(
            TrashItem(tenantId, userId, "Ada Lovelace", "active"),
            TrashItem(tenantId, userId, "Grace Hopper", "restored"),
            TrashItem(Guid.NewGuid(), userId, "Other Tenant", "active"));

        var handler = fixture.CreateHandler(tenantId, userId);
        var result = await handler.Handle(new GetGlobalTrashItemsQuery(), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].DisplayName.Should().Be("Ada Lovelace");
        result.Items[0].Status.Should().Be(CrmTrashStatuses.Active);
    }

    [Fact]
    public async Task Handle_ShouldFilterByEntityTypeAndSearch()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await TrashQueryFixture.CreateAsync(tenantId);

        await fixture.SeedAsync(
            TrashItem(tenantId, userId, "Ada Lovelace", CrmTrashStatuses.Active, CrmTrashEntityTypes.Contact),
            TrashItem(tenantId, userId, "Nikola Tesla", CrmTrashStatuses.Active, "lead"));

        var handler = fixture.CreateHandler(tenantId, userId);
        var result = await handler.Handle(
            new GetGlobalTrashItemsQuery(Search: "Ada", EntityType: CrmTrashEntityTypes.Contact),
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].EntityType.Should().Be(CrmTrashEntityTypes.Contact);
    }

    [Fact]
    public async Task Handle_ShouldApplyPagingAndDefaultSortDeletedAtDesc()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await TrashQueryFixture.CreateAsync(tenantId);

        await fixture.SeedAsync(
            TrashItem(tenantId, userId, "Old Item", CrmTrashStatuses.Active, deletedAtUtc: DateTime.UtcNow.AddDays(-3)),
            TrashItem(tenantId, userId, "New Item", CrmTrashStatuses.Active, deletedAtUtc: DateTime.UtcNow.AddDays(-1)),
            TrashItem(tenantId, userId, "Mid Item", CrmTrashStatuses.Active, deletedAtUtc: DateTime.UtcNow.AddDays(-2)));

        var handler = fixture.CreateHandler(tenantId, userId);
        var result = await handler.Handle(new GetGlobalTrashItemsQuery(Page: 1, PageSize: 2), CancellationToken.None);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(2);
        result.Items[0].DisplayName.Should().Be("New Item");
        result.Items[1].DisplayName.Should().Be("Mid Item");
    }

    private static GlobalTrashItem TrashItem(
        Guid tenantId,
        Guid userId,
        string displayName,
        string status,
        string entityType = CrmTrashEntityTypes.Contact,
        DateTime? deletedAtUtc = null)
    {
        var deletedAt = deletedAtUtc ?? DateTime.UtcNow;
        return new GlobalTrashItem
        {
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = Guid.NewGuid(),
            DisplayName = displayName,
            SourceModule = "contacts",
            OriginalRoute = "/contacts",
            DeletedAtUtc = deletedAt,
            DeletedByUserId = userId,
            DeletedByDisplayName = "unit-test",
            ExpiresAtUtc = deletedAt.AddDays(7),
            Status = status
        };
    }

    private sealed class TrashQueryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private TrashQueryFixture(SqliteConnection connection, CustomerManagementDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public CustomerManagementDbContext DbContext { get; }

        public static async Task<TrashQueryFixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new CustomerManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new TrashQueryFixture(connection, dbContext);
        }

        public GetGlobalTrashItemsQueryHandler CreateHandler(Guid tenantId, Guid userId)
            => new(
                DbContext,
                new FixedAuthorizationScope(new AuthorizationScope(
                    tenantId,
                    userId,
                    CrmAuthorizationCatalog.ContactsResource,
                    RowAccessLevel.Tenant,
                    [])));

        public async Task SeedAsync(params GlobalTrashItem[] items)
        {
            await DbContext.GlobalTrashItems.AddRangeAsync(items);
            await DbContext.SaveChangesAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class FixedAuthorizationScope(AuthorizationScope scope) : ICurrentAuthorizationScope
    {
        public AuthorizationScope Resolve(string resource) => scope with { Resource = resource };
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? TenantId => tenantId;
    }
}
