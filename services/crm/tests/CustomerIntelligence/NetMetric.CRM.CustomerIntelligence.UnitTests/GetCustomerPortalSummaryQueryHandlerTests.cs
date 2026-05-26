// <copyright file="GetCustomerPortalSummaryQueryHandlerTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Insights;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Insights.Queries.GetCustomerPortalSummary;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.AccountHierarchyNodes;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerHealthScores;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.DuplicateMatchs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.MergeJobs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.OwnershipHistoryEntrys;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.SavedViews;

namespace NetMetric.CRM.CustomerIntelligence.UnitTests;

public sealed class GetCustomerPortalSummaryQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Read_Health_And_Open_Counts_From_Json()
    {
        var customerId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();

        var row = CustomerHealthScore.Create("portal-summary", "Customer", customerId, """
            {
              "healthScore": 82.7,
              "openTickets": 4,
              "openOpportunities": 2,
              "openInvoices": 1
            }
            """);
        dbContext.CustomerHealthScores.Add(row);
        await dbContext.SaveChangesAsync();

        var handler = new GetCustomerPortalSummaryQueryHandler(
            dbContext,
            new StubCustomerPortalSummaryMetricsProvider(-1, -1, -1));
        var result = await handler.Handle(new GetCustomerPortalSummaryQuery(customerId), CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.HealthScore.Should().Be(82.7m);
        result.OpenTickets.Should().Be(4);
        result.OpenOpportunities.Should().Be(2);
        result.OpenInvoices.Should().Be(1);
    }

    [Fact]
    public async Task Handle_Should_Fallback_To_Zero_When_No_Health_Row_Exists()
    {
        var customerId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();

        var handler = new GetCustomerPortalSummaryQueryHandler(
            dbContext,
            new StubCustomerPortalSummaryMetricsProvider(-1, -1, -1));
        var result = await handler.Handle(new GetCustomerPortalSummaryQuery(customerId), CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.HealthScore.Should().Be(0m);
        result.OpenTickets.Should().Be(0);
        result.OpenOpportunities.Should().Be(0);
        result.OpenInvoices.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Prefer_Timeline_Based_Open_Counts_When_Available()
    {
        var customerId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();

        dbContext.CustomerHealthScores.Add(
            CustomerHealthScore.Create(
                "Acme Corp",
                "Customer",
                customerId,
                """{ "healthScore": 55, "openTickets": 9, "openOpportunities": 9, "openInvoices": 9 }"""));

        dbContext.CustomerTimelineEntrys.AddRange(
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Ticket A",
                Category = "Ticket",
                EntityType = "Ticket",
                DataJson = """{ "status": "Open" }""",
            },
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Ticket B",
                Category = "Ticket",
                EntityType = "Ticket",
                DataJson = """{ "status": "Resolved" }""",
            },
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Opp A",
                Category = "Opportunity",
                EntityType = "Opportunity",
                DataJson = """{ "status": "Open" }""",
            },
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Opp B",
                Category = "Opportunity",
                EntityType = "Opportunity",
                DataJson = """{ "status": "Closed-Won" }""",
            },
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Invoice A",
                Category = "Invoice",
                EntityType = "Invoice",
                DataJson = """{ "status": "Pending" }""",
            },
            new CustomerTimelineEntry
            {
                SubjectType = "Customer",
                SubjectId = customerId,
                Name = "Invoice B",
                Category = "Invoice",
                EntityType = "Invoice",
                DataJson = """{ "status": "Paid" }""",
            });

        await dbContext.SaveChangesAsync();

        var handler = new GetCustomerPortalSummaryQueryHandler(
            dbContext,
            new StubCustomerPortalSummaryMetricsProvider(-1, -1, -1));
        var result = await handler.Handle(new GetCustomerPortalSummaryQuery(customerId), CancellationToken.None);

        result.HealthScore.Should().Be(55m);
        result.OpenTickets.Should().Be(1);
        result.OpenOpportunities.Should().Be(1);
        result.OpenInvoices.Should().Be(1);
        result.DisplayName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task Handle_Should_Prefer_Provider_Metrics_When_Available()
    {
        var customerId = Guid.NewGuid();
        await using var dbContext = CreateDbContext();

        dbContext.CustomerHealthScores.Add(
            CustomerHealthScore.Create("Acme Corp", "Customer", customerId, """{ "healthScore": 70 }"""));
        await dbContext.SaveChangesAsync();

        var handler = new GetCustomerPortalSummaryQueryHandler(
            dbContext,
            new StubCustomerPortalSummaryMetricsProvider(8, 6, 3));

        var result = await handler.Handle(new GetCustomerPortalSummaryQuery(customerId), CancellationToken.None);

        result.OpenTickets.Should().Be(8);
        result.OpenOpportunities.Should().Be(6);
        result.OpenInvoices.Should().Be(3);
    }

    private static TestCustomerIntelligenceDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TestCustomerIntelligenceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;
        return new TestCustomerIntelligenceDbContext(options);
    }

    private sealed class TestCustomerIntelligenceDbContext(DbContextOptions<TestCustomerIntelligenceDbContext> options)
        : DbContext(options), ICustomerIntelligenceDbContext
    {
        public DbSet<DuplicateMatch> DuplicateMatchs => Set<DuplicateMatch>();
        public DbSet<MergeJob> MergeJobs => Set<MergeJob>();
        public DbSet<AccountHierarchyNode> AccountHierarchyNodes => Set<AccountHierarchyNode>();
        public DbSet<CustomerTimelineEntry> CustomerTimelineEntrys => Set<CustomerTimelineEntry>();
        public DbSet<SavedView> SavedViews => Set<SavedView>();
        public DbSet<OwnershipHistoryEntry> OwnershipHistoryEntrys => Set<OwnershipHistoryEntry>();
        public DbSet<CustomerRelationship> CustomerRelationships => Set<CustomerRelationship>();
        public DbSet<CustomerHealthScore> CustomerHealthScores => Set<CustomerHealthScore>();
        public DbSet<BehavioralEvent> BehavioralEvents => Set<BehavioralEvent>();
        public DbSet<IdentityProfile> IdentityProfiles => Set<IdentityProfile>();
    }

    private sealed class StubCustomerPortalSummaryMetricsProvider(
        int openTickets,
        int openOpportunities,
        int openInvoices) : ICustomerPortalSummaryMetricsProvider
    {
        public Task<CustomerPortalSummaryMetrics> GetMetricsAsync(Guid customerId, CancellationToken cancellationToken)
            => Task.FromResult(new CustomerPortalSummaryMetrics(openTickets, openOpportunities, openInvoices));
    }
}
