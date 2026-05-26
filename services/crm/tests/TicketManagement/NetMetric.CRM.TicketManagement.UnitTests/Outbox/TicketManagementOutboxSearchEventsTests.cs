// <copyright file="TicketManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketManagement.UnitTests.Outbox;

public sealed class TicketManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueueTicketCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var ticket = CreateTicket(tenantId);
        var outbox = new TicketManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueTicketCreatedAsync(ticket, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("ticket");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueTicketUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var ticket = CreateTicket(tenantId);
        var outbox = new TicketManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueTicketUpdatedAsync(ticket, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("ticket");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueTicketDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var ticket = CreateTicket(tenantId);
        var outbox = new TicketManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueTicketDeletedAsync(ticket, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("ticket");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(TicketSearchIntegrationEventFactory.BuildDocumentId(tenantId, ticket.Id));
    }

    private static Ticket CreateTicket(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            TicketNumber = "TKT-2026-0002",
            Subject = "Search event test",
            Description = "Internal-only details should not be indexed.",
            TicketType = TicketType.Support,
            Channel = TicketChannelType.Web,
            Priority = PriorityType.Medium,
            Status = TicketStatusType.New,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture(SqliteConnection connection, TicketManagementDbContext dbContext, ICurrentUserService currentUser) : IAsyncDisposable
    {
        public TicketManagementDbContext DbContext { get; } = dbContext;
        public ICurrentUserService CurrentUser { get; } = currentUser;

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<TicketManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new TicketManagementDbContext(
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
