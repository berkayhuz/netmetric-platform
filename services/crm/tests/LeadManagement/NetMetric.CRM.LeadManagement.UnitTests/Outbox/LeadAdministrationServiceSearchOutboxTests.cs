// <copyright file="LeadAdministrationServiceSearchOutboxTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetMetric.Clock;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.LeadManagement.Infrastructure.Services;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.LeadManagement.UnitTests.Outbox;

public sealed class LeadAdministrationServiceSearchOutboxTests
{
    [Fact]
    public async Task CreateAsync_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new Mock<ILeadManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueLeadCreatedAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object, new Mock<IOpportunityCrossWriterService>(MockBehavior.Loose).Object);
        var command = new CreateLeadCommand(
            "Ada Lovelace",
            "Analytical Engines Ltd",
            "ada@example.com",
            "555-11-22",
            "CTO",
            "private description",
            5000m,
            DateTime.UtcNow.AddDays(5),
            LeadSourceType.Manual,
            LeadStatusType.New,
            PriorityType.Medium,
            null,
            null,
            "private note");

        var result = await sut.CreateAsync(command, CancellationToken.None);

        result.FullName.Should().Be("Ada Lovelace");
        outbox.Verify(x => x.EnqueueLeadCreatedAsync(It.Is<Lead>(l => l.Id == result.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedLeadAsync();
        var outbox = new Mock<ILeadManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueLeadUpdatedAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object, new Mock<IOpportunityCrossWriterService>(MockBehavior.Loose).Object);
        var command = new UpdateLeadCommand(
            seeded.Id,
            "Grace Hopper",
            "Compilers Inc",
            "grace@example.com",
            "555-22-33",
            "Engineer",
            "updated description",
            9000m,
            DateTime.UtcNow.AddDays(3),
            LeadSourceType.Manual,
            LeadStatusType.Contacted,
            PriorityType.High,
            null,
            null,
            "updated note",
            Convert.ToBase64String(seeded.RowVersion));

        var result = await sut.UpdateAsync(command, CancellationToken.None);

        result.FullName.Should().Be("Grace Hopper");
        outbox.Verify(x => x.EnqueueLeadUpdatedAsync(It.Is<Lead>(l => l.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_Should_Enqueue_Search_Delete_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedLeadAsync();
        var outbox = new Mock<ILeadManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueLeadDeletedAsync(It.IsAny<Lead>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object, new Mock<IOpportunityCrossWriterService>(MockBehavior.Loose).Object);

        await sut.SoftDeleteAsync(new SoftDeleteLeadCommand(seeded.Id), CancellationToken.None);

        outbox.Verify(x => x.EnqueueLeadDeletedAsync(It.Is<Lead>(l => l.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_Should_Throw_ValidationAppException_When_FullName_Is_Missing()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new Mock<ILeadManagementOutbox>(MockBehavior.Strict);

        var sut = fixture.CreateService(outbox.Object, new Mock<IOpportunityCrossWriterService>(MockBehavior.Loose).Object);
        var command = new CreateLeadCommand(
            null!,
            "Analytical Engines Ltd",
            null,
            null,
            null,
            null,
            null,
            null,
            LeadSourceType.Manual,
            LeadStatusType.New,
            PriorityType.Medium,
            null,
            null,
            null);

        var act = () => sut.CreateAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationAppException>()
            .WithMessage("*FullName is required*");

        outbox.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ConvertToCustomerAsync_Should_Create_Opportunity_In_Authoritative_CrossWriter_Path()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedLeadAsync();
        var outbox = new Mock<ILeadManagementOutbox>(MockBehavior.Loose);
        var opportunityCrossWriter = new Mock<IOpportunityCrossWriterService>(MockBehavior.Strict);
        var createdOpportunityId = Guid.NewGuid();
        opportunityCrossWriter
            .Setup(x => x.CreateAsync(It.IsAny<OpportunityCrossWriterCreateRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdOpportunityId);

        var sut = fixture.CreateService(outbox.Object, opportunityCrossWriter.Object);

        var result = await sut.ConvertToCustomerAsync(
            new ConvertLeadToCustomerCommand(seeded.Id, CustomerType.Individual, true, true, "Converted Opportunity", 1200m, null),
            CancellationToken.None);

        result.OpportunityId.Should().Be(createdOpportunityId);
        fixture.DbContext.Opportunities.Count().Should().Be(0);
        opportunityCrossWriter.Verify(x => x.CreateAsync(It.IsAny<OpportunityCrossWriterCreateRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        public LeadManagementDbContext DbContext { get; }
        private readonly ICurrentUserService currentUser;

        private Fixture(LeadManagementDbContext dbContext, ICurrentUserService currentUser)
        {
            DbContext = dbContext;
            this.currentUser = currentUser;
        }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var options = new DbContextOptionsBuilder<LeadManagementDbContext>()
                .UseInMemoryDatabase($"lead-admin-service-{Guid.NewGuid():N}")
                .Options;

            var dbContext = new LeadManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(dbContext, new FixedCurrentUser(tenantId));
        }

        public LeadAdministrationService CreateService(ILeadManagementOutbox outbox, IOpportunityCrossWriterService opportunityCrossWriterService)
        {
            var clock = new Mock<IClock>();
            clock.SetupGet(x => x.UtcDateTime).Returns(DateTime.UtcNow);
            return new LeadAdministrationService(DbContext, currentUser, clock.Object, outbox, opportunityCrossWriterService);
        }

        public async Task<Lead> SeedLeadAsync()
        {
            var lead = new Lead
            {
                TenantId = currentUser.TenantId,
                LeadCode = "LEAD-2026-0999",
                FullName = "Seed Lead",
                Source = LeadSourceType.Manual,
                Status = LeadStatusType.New,
                Priority = PriorityType.Medium,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                UpdatedBy = "tester"
            };

            DbContext.Leads.Add(lead);
            await DbContext.SaveChangesAsync();
            return lead;
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
