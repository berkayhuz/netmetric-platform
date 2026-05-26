// <copyright file="LeadManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.LeadManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.LeadManagement.UnitTests.Outbox;

public sealed class LeadManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueueLeadCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var lead = CreateLead(tenantId);
        var outbox = new LeadManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueLeadCreatedAsync(lead, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("lead");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueLeadUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var lead = CreateLead(tenantId);
        var outbox = new LeadManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueLeadUpdatedAsync(lead, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("lead");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueLeadDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var lead = CreateLead(tenantId);
        var outbox = new LeadManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueLeadDeletedAsync(lead, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("lead");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(LeadSearchIntegrationEventFactory.BuildDocumentId(tenantId, lead.Id));
    }

    private static Lead CreateLead(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            LeadCode = "LEAD-2026-0002",
            FullName = "Search Event Lead",
            CompanyName = "Event Co",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture(LeadManagementDbContext dbContext, ICurrentUserService currentUser) : IAsyncDisposable
    {
        public LeadManagementDbContext DbContext { get; } = dbContext;
        public ICurrentUserService CurrentUser { get; } = currentUser;

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var options = new DbContextOptionsBuilder<LeadManagementDbContext>()
                .UseInMemoryDatabase($"lead-outbox-{Guid.NewGuid():N}")
                .Options;

            var dbContext = new LeadManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
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
