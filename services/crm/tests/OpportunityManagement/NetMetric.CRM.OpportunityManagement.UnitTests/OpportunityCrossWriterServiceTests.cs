// <copyright file="OpportunityCrossWriterServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.Tenancy;

namespace NetMetric.CRM.OpportunityManagement.UnitTests;

public sealed class OpportunityCrossWriterServiceTests
{
    [Fact]
    public async Task CreateAsync_Should_Persist_Opportunity_Into_OpportunityManagementDbContext()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new RecordingOpportunityManagementOutbox();
        var sut = new OpportunityCrossWriterService(fixture.DbContext, outbox);

        var opportunityId = await sut.CreateAsync(
            new OpportunityCrossWriterCreateRequest(
                tenantId,
                "OPP-CW-001",
                "Cross Writer Opportunity",
                "desc",
                100m,
                120m,
                25m,
                DateTime.UtcNow.AddDays(15),
                OpportunityStageType.Prospecting,
                OpportunityStatusType.Open,
                PriorityType.Medium,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "tester"),
            CancellationToken.None);

        var created = await fixture.DbContext.Opportunities.FirstOrDefaultAsync(x => x.Id == opportunityId);
        created.Should().NotBeNull();
        created!.TenantId.Should().Be(tenantId);
        created.Name.Should().Be("Cross Writer Opportunity");
        outbox.Created.Should().ContainSingle().Which.Id.Should().Be(opportunityId);
    }

    [Fact]
    public async Task ChangeStageAsync_Should_Update_Authoritative_Opportunity_In_OpportunityManagementDbContext()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new RecordingOpportunityManagementOutbox();
        var sut = new OpportunityCrossWriterService(fixture.DbContext, outbox);

        var opportunityId = await sut.CreateAsync(
            new OpportunityCrossWriterCreateRequest(
                tenantId,
                "OPP-CW-002",
                "Stage Change Opportunity",
                null,
                200m,
                220m,
                30m,
                null,
                OpportunityStageType.Prospecting,
                OpportunityStatusType.Open,
                PriorityType.Medium,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                "tester"),
            CancellationToken.None);

        var before = await fixture.DbContext.Opportunities.FirstAsync(x => x.Id == opportunityId);
        var rowVersion = Convert.ToBase64String(before.RowVersion);

        var result = await sut.ChangeStageAsync(
            new OpportunityCrossWriterStageChangeRequest(
                tenantId,
                opportunityId,
                OpportunityStageType.Won,
                OpportunityStatusType.Won,
                null,
                null,
                "won",
                null,
                null,
                80m,
                ForecastCategory.Commit,
                rowVersion,
                Guid.NewGuid(),
                "tester"),
            CancellationToken.None);

        result.CurrentStage.Should().Be(OpportunityStageType.Won);
        var updated = await fixture.DbContext.Opportunities.FirstAsync(x => x.Id == opportunityId);
        updated.Stage.Should().Be(OpportunityStageType.Won);
        updated.Status.Should().Be(OpportunityStatusType.Won);
        updated.ForecastCategory.Should().Be(ForecastCategory.Commit);
        outbox.Updated.Should().ContainSingle().Which.Id.Should().Be(opportunityId);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public OpportunityManagementDbContext DbContext { get; }

        private Fixture(OpportunityManagementDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var options = new DbContextOptionsBuilder<OpportunityManagementDbContext>()
                .UseInMemoryDatabase($"opportunity-cross-writer-{Guid.NewGuid():N}")
                .Options;
            var dbContext = new OpportunityManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(dbContext);
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

    private sealed class RecordingOpportunityManagementOutbox : IOpportunityManagementOutbox
    {
        public List<NetMetric.CRM.Sales.Opportunity> Created { get; } = [];
        public List<NetMetric.CRM.Sales.Opportunity> Updated { get; } = [];

        public Task EnqueueOpportunityCreatedAsync(NetMetric.CRM.Sales.Opportunity opportunity, CancellationToken cancellationToken)
        {
            Created.Add(opportunity);
            return Task.CompletedTask;
        }

        public Task EnqueueOpportunityCreatedAndPersistAsync(NetMetric.CRM.Sales.Opportunity opportunity, CancellationToken cancellationToken)
        {
            Created.Add(opportunity);
            return Task.CompletedTask;
        }

        public Task EnqueueOpportunityUpdatedAsync(NetMetric.CRM.Sales.Opportunity opportunity, CancellationToken cancellationToken)
        {
            Updated.Add(opportunity);
            return Task.CompletedTask;
        }

        public Task EnqueueOpportunityDeletedAsync(NetMetric.CRM.Sales.Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityRestoredAsync(NetMetric.CRM.Sales.Opportunity opportunity, CancellationToken cancellationToken) => Task.CompletedTask;
        public Task EnqueueOpportunityPurgedAsync(Guid tenantId, Guid opportunityId, string? opportunityName, Guid? ownerUserId, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
