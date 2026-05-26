// <copyright file="OpportunityCrossWriterRoutingTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetMetric.CRM.Core;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Services;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Application.Handlers;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Tenancy;

namespace NetMetric.CRM.PipelineManagement.UnitTests;

public sealed class OpportunityCrossWriterRoutingTests
{
    [Fact]
    public async Task ConvertLeadCommandHandler_Should_Create_Opportunity_Through_Authoritative_CrossWriter()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var lead = await fixture.SeedLeadAsync();
        var customer = await fixture.SeedCustomerAsync();

        var crossWriter = new Mock<IOpportunityCrossWriterService>(MockBehavior.Strict);
        var createdOpportunityId = Guid.NewGuid();
        crossWriter.Setup(x => x.CreateAsync(It.IsAny<OpportunityCrossWriterCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOpportunityId);

        var sut = new ConvertLeadCommandHandler(fixture.DbContext, fixture.CurrentUser, crossWriter.Object);
        var result = await sut.Handle(
            new ConvertLeadCommand(lead.Id, false, true, customer.Id, "Cross Writer Opp", 1500m, OpportunityStageType.Prospecting, PriorityType.Medium, null, null),
            CancellationToken.None);

        result.OpportunityId.Should().Be(createdOpportunityId);
        fixture.DbContext.Opportunities.Count().Should().Be(0);
        crossWriter.Verify(x => x.CreateAsync(It.IsAny<OpportunityCrossWriterCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ChangeOpportunityStageCommandHandler_Should_Route_Authoritative_Update_Through_CrossWriter()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var opportunity = await fixture.SeedOpportunityAsync();

        var crossWriter = new Mock<IOpportunityCrossWriterService>(MockBehavior.Strict);
        crossWriter.Setup(x => x.ChangeStageAsync(It.IsAny<OpportunityCrossWriterStageChangeRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpportunityCrossWriterStageChangeResult(
                opportunity.Id,
                OpportunityStageType.Prospecting,
                OpportunityStageType.Qualification,
                OpportunityStatusType.Open,
                null,
                null,
                Convert.ToBase64String(opportunity.RowVersion)));

        var validation = new Mock<IPipelineValidationService>(MockBehavior.Strict);
        validation
            .Setup(x => x.ValidateStageTransitionAsync(It.IsAny<Opportunity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, new List<string>()));

        var sut = new ChangeOpportunityStageCommandHandler(fixture.DbContext, fixture.CurrentUser, crossWriter.Object, validation.Object);
        await sut.Handle(
            new ChangeOpportunityStageCommand(opportunity.Id, OpportunityStageType.Qualification, null, "update", null, null, null),
            CancellationToken.None);

        crossWriter.Verify(x => x.ChangeStageAsync(It.IsAny<OpportunityCrossWriterStageChangeRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public PipelineManagementDbContext DbContext { get; }
        public ICurrentUserService CurrentUser { get; }

        private Fixture(SqliteConnection connection, PipelineManagementDbContext dbContext, ICurrentUserService currentUser)
        {
            this.connection = connection;
            DbContext = dbContext;
            CurrentUser = currentUser;
        }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<PipelineManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new PipelineManagementDbContext(options, new FixedTenantContext(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(connection, dbContext, new FixedCurrentUser(tenantId));
        }

        public async Task<Lead> SeedLeadAsync()
        {
            var lead = new Lead
            {
                TenantId = CurrentUser.TenantId,
                LeadCode = "LEAD-PIPE-1",
                FullName = "Pipeline Lead",
                Source = LeadSourceType.Manual,
                Status = LeadStatusType.New,
                Priority = PriorityType.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            DbContext.Leads.Add(lead);
            await DbContext.SaveChangesAsync();
            return lead;
        }

        public async Task<Customer> SeedCustomerAsync()
        {
            var customer = new Customer
            {
                TenantId = CurrentUser.TenantId,
                FirstName = "C",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            DbContext.Customers.Add(customer);
            await DbContext.SaveChangesAsync();
            return customer;
        }

        public async Task<Opportunity> SeedOpportunityAsync()
        {
            var opportunity = new Opportunity
            {
                TenantId = CurrentUser.TenantId,
                OpportunityCode = "OPP-PIPE-1",
                Name = "Pipeline Opportunity",
                EstimatedAmount = 100m,
                Probability = 25m,
                Stage = OpportunityStageType.Prospecting,
                Status = OpportunityStatusType.Open,
                Priority = PriorityType.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            DbContext.Opportunities.Add(opportunity);
            await DbContext.SaveChangesAsync();
            return opportunity;
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    private sealed class FixedTenantContext(Guid tenantId) : ITenantContext
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
