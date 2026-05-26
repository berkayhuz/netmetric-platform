// <copyright file="DealManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence;
using NetMetric.CRM.DealManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DealManagement.UnitTests.Outbox;

public sealed class DealManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueueDealCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var deal = CreateDeal(tenantId);
        var outbox = new DealManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueDealCreatedAsync(deal, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("deal");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueDealUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var deal = CreateDeal(tenantId);
        var outbox = new DealManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueDealUpdatedAsync(deal, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("deal");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueDealDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var deal = CreateDeal(tenantId);
        var outbox = new DealManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueDealDeletedAsync(deal, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("deal");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(DealSearchIntegrationEventFactory.BuildDocumentId(tenantId, deal.Id));
    }

    private static Deal CreateDeal(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            DealCode = "DEAL-2026-2002",
            Name = "Search event deal",
            TotalAmount = 1000m,
            ClosedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture(SqliteConnection connection, DealManagementDbContext dbContext, ICurrentUserService currentUser) : IAsyncDisposable
    {
        public DealManagementDbContext DbContext { get; } = dbContext;
        public ICurrentUserService CurrentUser { get; } = currentUser;

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
