// <copyright file="MarkOpportunityWonCommandHandlerTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using NetMetric.CRM.Core;
using NetMetric.CRM.Activities;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Domain.Entities;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Commands;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.OpportunityManagement.UnitTests;

public sealed class MarkOpportunityWonCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateDealAndRelaySearchOutboxMessage()
    {
        var tenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<TestOpportunityManagementDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dealConnection = new SqliteConnection("Data Source=:memory:");
        await dealConnection.OpenAsync();
        var dealOptions = new DbContextOptionsBuilder<TestDealManagementDbContext>()
            .UseSqlite(dealConnection)
            .Options;

        await using var dbContext = new TestOpportunityManagementDbContext(options);
        await using var dealDbContext = new TestDealManagementDbContext(dealOptions);
        await dbContext.Database.EnsureCreatedAsync();
        await dealDbContext.Database.EnsureCreatedAsync();
        var seededOpportunity = new Opportunity
        {
            TenantId = tenantId,
            OpportunityCode = "OPP-001",
            Name = "Won Opportunity",
            EstimatedAmount = 1000m,
            ExpectedRevenue = 1200m,
            Probability = 50m,
            Stage = OpportunityStageType.Qualification,
            Status = OpportunityStatusType.Open,
            Priority = PriorityType.Medium,
            CreatedAt = now,
            UpdatedAt = now
        };
        dbContext.Opportunities.Add(seededOpportunity);
        await dbContext.SaveChangesAsync();

        var outbox = new RecordingDealManagementOutbox();
        var opportunityOutbox = new RecordingOpportunityManagementOutbox();
        var currentUser = new TestCurrentUserService(tenantId);
        var handler = new MarkOpportunityWonCommandHandler(dbContext, currentUser, dealDbContext, outbox, opportunityOutbox);

        var result = await handler.Handle(
            new MarkOpportunityWonCommand(seededOpportunity.Id, "Won Deal", now.Date, null),
            CancellationToken.None);

        result.Should().NotBeNull();
        dbContext.Deals.Should().BeEmpty();
        dealDbContext.Deals.Should().ContainSingle();

        var createdDeal = dealDbContext.Deals.Single();
        createdDeal.OpportunityId.Should().BeNull();
        createdDeal.Name.Should().Be("Won Deal");
        createdDeal.TenantId.Should().Be(tenantId);

        outbox.CreatedAndPersistedDeals.Should().ContainSingle()
            .Which.Id.Should().Be(createdDeal.Id);
    }

    private sealed class TestOpportunityManagementDbContext(DbContextOptions<TestOpportunityManagementDbContext> options)
        : DbContext(options), IOpportunityManagementDbContext
    {
        public DbSet<Opportunity> Opportunities => Set<Opportunity>();
        public DbSet<OpportunityProduct> OpportunityProducts => Set<OpportunityProduct>();
        public DbSet<OpportunityContact> OpportunityContacts => Set<OpportunityContact>();
        public DbSet<OpportunityStageHistory> OpportunityStageHistories => Set<OpportunityStageHistory>();
        public DbSet<Quote> Quotes => Set<Quote>();
        public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
        public DbSet<LostReason> LostReasons => Set<LostReason>();
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    }

    private sealed class TestDealManagementDbContext(DbContextOptions<TestDealManagementDbContext> options)
        : DbContext(options), IDealManagementDbContext
    {
        public DbSet<Deal> Deals => Set<Deal>();
        public DbSet<LostReason> LostReasons => Set<LostReason>();
        public DbSet<DealOutcomeHistory> DealOutcomeHistories => Set<DealOutcomeHistory>();
        public DbSet<WinLossReview> WinLossReviews => Set<WinLossReview>();
        public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    }

    private sealed class RecordingDealManagementOutbox : IDealManagementOutbox
    {
        public List<Deal> CreatedAndPersistedDeals { get; } = [];

        public Task EnqueueDealCreatedAsync(Deal deal, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task EnqueueDealUpdatedAsync(Deal deal, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task EnqueueDealDeletedAsync(Deal deal, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task EnqueueDealRestoredAsync(Deal deal, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueDealPurgedAsync(Guid tenantId, Guid dealId, string? dealName, Guid? ownerUserId, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task EnqueueDealCreatedAndPersistAsync(Deal deal, CancellationToken cancellationToken)
        {
            CreatedAndPersistedDeals.Add(deal);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingOpportunityManagementOutbox : IOpportunityManagementOutbox
    {
        public Task EnqueueOpportunityCreatedAsync(Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityCreatedAndPersistAsync(Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityUpdatedAsync(Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityDeletedAsync(Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityRestoredAsync(Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityPurgedAsync(Guid tenantId, Guid opportunityId, string? opportunityName, Guid? ownerUserId, CancellationToken cancellationToken) => Task.CompletedTask;
    }

    private sealed class TestCurrentUserService(Guid tenantId) : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated { get; } = true;
        public string? UserName { get; } = "opportunity-test";
        public string? Email { get; } = "opportunity-test@netmetric.local";
        public IReadOnlyCollection<string> Roles { get; } = [];
        public IReadOnlyCollection<string> Permissions { get; } = [];
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
    }
}
