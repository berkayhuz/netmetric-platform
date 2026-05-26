// <copyright file="PipelineManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CRM.PipelineManagement.Infrastructure.Services;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.PipelineManagement.UnitTests.Outbox;

public sealed class PipelineManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueuePipelineCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var pipeline = CreatePipeline(tenantId);
        var outbox = new PipelineManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueuePipelineCreatedAsync(pipeline, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("pipeline");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueuePipelineUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var pipeline = CreatePipeline(tenantId);
        var outbox = new PipelineManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueuePipelineUpdatedAsync(pipeline, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("pipeline");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueuePipelineDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var pipeline = CreatePipeline(tenantId);
        var outbox = new PipelineManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueuePipelineDeletedAsync(pipeline, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("pipeline");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(PipelineSearchIntegrationEventFactory.BuildDocumentId(tenantId, pipeline.Id));
    }

    private static Pipeline CreatePipeline(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            Name = "Search Event Pipeline",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture(SqliteConnection connection, PipelineManagementDbContext dbContext, ICurrentUserService currentUser) : IAsyncDisposable
    {
        public PipelineManagementDbContext DbContext { get; } = dbContext;
        public ICurrentUserService CurrentUser { get; } = currentUser;

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<PipelineManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new PipelineManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(connection, dbContext, new FixedCurrentUser(tenantId));
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantContext
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
