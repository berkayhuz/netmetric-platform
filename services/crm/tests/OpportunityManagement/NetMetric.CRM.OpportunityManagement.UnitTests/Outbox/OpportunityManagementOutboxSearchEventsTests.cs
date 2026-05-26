// <copyright file="OpportunityManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.OpportunityManagement.UnitTests.Outbox;

public sealed class OpportunityManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueueOpportunityCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = Fixture.Create(tenantId);
        var opportunity = CreateOpportunity(tenantId);
        var outbox = new OpportunityManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueOpportunityCreatedAsync(opportunity, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local.Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");
        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("opportunity");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueOpportunityUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = Fixture.Create(tenantId);
        var opportunity = CreateOpportunity(tenantId);
        var outbox = new OpportunityManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueOpportunityUpdatedAsync(opportunity, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local.Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");
        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("opportunity");
    }

    [Fact]
    public async Task EnqueueOpportunityDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = Fixture.Create(tenantId);
        var opportunity = CreateOpportunity(tenantId);
        var outbox = new OpportunityManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueOpportunityDeletedAsync(opportunity, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local.Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");
        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Type.Should().Be("opportunity");
        integrationEvent.DocumentId.Should().Be(OpportunitySearchIntegrationEventFactory.BuildDocumentId(tenantId, opportunity.Id));
    }

    private static Opportunity CreateOpportunity(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            OpportunityCode = "OPP-2026-2002",
            Name = "Search event opportunity",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture : IAsyncDisposable
    {
        private Fixture(OpportunityManagementDbContext dbContext, ICurrentUserService currentUser)
        {
            DbContext = dbContext;
            CurrentUser = currentUser;
        }

        public OpportunityManagementDbContext DbContext { get; }
        public ICurrentUserService CurrentUser { get; }

        public static Fixture Create(Guid tenantId)
        {
            var options = new DbContextOptionsBuilder<OpportunityManagementDbContext>()
                .UseInMemoryDatabase($"opportunity-outbox-{Guid.NewGuid():N}")
                .Options;
            var dbContext = new OpportunityManagementDbContext(options, new FixedTenantProvider(tenantId));
            return new Fixture(dbContext, new FixedCurrentUser(tenantId));
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
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
