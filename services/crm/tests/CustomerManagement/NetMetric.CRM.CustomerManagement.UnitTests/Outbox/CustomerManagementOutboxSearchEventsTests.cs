// <copyright file="CustomerManagementOutboxSearchEventsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Outbox;

public sealed class CustomerManagementOutboxSearchEventsTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task EnqueueCustomerUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var customer = CreateCustomer(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueCustomerUpdatedAsync(customer, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("customer");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueCustomerDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var customer = CreateCustomer(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueCustomerDeletedAsync(customer, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("customer");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(CustomerSearchIntegrationEventFactory.BuildDocumentId(tenantId, customer.Id));
    }

    [Fact]
    public async Task EnqueueCompanyUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var company = CreateCompany(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueCompanyUpdatedAsync(company, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("company");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.Visibility.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentVisibility.Permission);
    }

    [Fact]
    public async Task EnqueueCompanyCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var company = CreateCompany(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueCompanyCreatedAsync(company, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("company");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueCompanyDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var company = CreateCompany(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueCompanyDeletedAsync(company, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("company");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(CompanySearchIntegrationEventFactory.BuildDocumentId(tenantId, company.Id));
    }

    [Fact]
    public async Task EnqueueContactUpdatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var contact = CreateContact(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueContactUpdatedAsync(contact, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("contact");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.Visibility.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentVisibility.Permission);
    }

    [Fact]
    public async Task EnqueueContactCreatedAsync_Should_Add_SearchIndex_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var contact = CreateContact(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueContactCreatedAsync(contact, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentIndexRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.index.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentIndexRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Document.Type.Should().Be("contact");
        integrationEvent.Document.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task EnqueueContactDeletedAsync_Should_Add_SearchDelete_OutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var contact = CreateContact(tenantId);
        var outbox = new CustomerManagementOutbox(fixture.DbContext, fixture.CurrentUser);

        await outbox.EnqueueContactDeletedAsync(contact, CancellationToken.None);
        var message = fixture.DbContext.OutboxMessages.Local
            .Single(x => x.EventName == SearchDocumentDeleteRequestedV1.EventName);

        message.RoutingKey.Should().Be("search.delete.crm");

        var integrationEvent = JsonSerializer.Deserialize<SearchDocumentDeleteRequestedV1>(message.PayloadJson, SerializerOptions);
        integrationEvent.Should().NotBeNull();
        integrationEvent!.Source.Should().Be(NetMetric.Search.Contracts.Documents.SearchDocumentSource.Crm);
        integrationEvent.Type.Should().Be("contact");
        integrationEvent.TenantId.Should().Be(tenantId);
        integrationEvent.DocumentId.Should().Be(ContactSearchIntegrationEventFactory.BuildDocumentId(tenantId, contact.Id));
    }

    private static Customer CreateCustomer(Guid tenantId) =>
        new()
        {
            TenantId = tenantId,
            FirstName = "Jane",
            LastName = "Doe",
            CustomerType = CustomerType.Corporate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static Company CreateCompany(Guid tenantId) =>
        new()
        {
            TenantId = tenantId,
            Name = "Contoso Holding",
            CompanyType = CompanyType.Prospect,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private static Contact CreateContact(Guid tenantId) =>
        new()
        {
            TenantId = tenantId,
            FirstName = "Ada",
            LastName = "Lovelace",
            Gender = GenderType.Unknown,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class Fixture(SqliteConnection connection, CustomerManagementDbContext dbContext, ICurrentUserService currentUser) : IAsyncDisposable
    {
        public CustomerManagementDbContext DbContext { get; } = dbContext;
        public ICurrentUserService CurrentUser { get; } = currentUser;

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new CustomerManagementDbContext(options, new FixedTenantProvider(tenantId));
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
