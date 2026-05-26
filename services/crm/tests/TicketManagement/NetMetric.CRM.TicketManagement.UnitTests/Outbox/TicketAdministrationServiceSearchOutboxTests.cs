// <copyright file="TicketAdministrationServiceSearchOutboxTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetMetric.Clock;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.TicketManagement.UnitTests.Outbox;

public sealed class TicketAdministrationServiceSearchOutboxTests
{
    [Fact]
    public async Task CreateAsync_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new Mock<ITicketManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueTicketCreatedAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object);
        var command = new CreateTicketCommand(
            "Search index create",
            "internal details",
            TicketType.Support,
            TicketChannelType.Web,
            PriorityType.High,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        var result = await sut.CreateAsync(command, CancellationToken.None);

        result.Subject.Should().Be("Search index create");
        outbox.Verify(x => x.EnqueueTicketCreatedAsync(It.Is<Ticket>(t => t.Id == result.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedTicketAsync();
        var outbox = new Mock<ITicketManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueTicketUpdatedAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object);
        var command = new UpdateTicketCommand(
            seeded.Id,
            "Search index update",
            "new description",
            TicketType.Support,
            TicketChannelType.Email,
            PriorityType.Medium,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            seeded.RowVersion);

        var result = await sut.UpdateAsync(command, CancellationToken.None);

        result.Subject.Should().Be("Search index update");
        outbox.Verify(x => x.EnqueueTicketUpdatedAsync(It.Is<Ticket>(t => t.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteAsync_Should_Enqueue_Search_Delete_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedTicketAsync();
        var outbox = new Mock<ITicketManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueueTicketDeletedAsync(It.IsAny<Ticket>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var sut = fixture.CreateService(outbox.Object);

        await sut.SoftDeleteAsync(new SoftDeleteTicketCommand(seeded.Id), CancellationToken.None);

        outbox.Verify(x => x.EnqueueTicketDeletedAsync(It.Is<Ticket>(t => t.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public TicketManagementDbContext DbContext { get; }
        private readonly ICurrentUserService currentUser;

        private Fixture(SqliteConnection connection, TicketManagementDbContext dbContext, ICurrentUserService currentUser)
        {
            this.connection = connection;
            DbContext = dbContext;
            this.currentUser = currentUser;
        }

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

        public TicketAdministrationService CreateService(ITicketManagementOutbox outbox)
        {
            var clock = new Mock<IClock>();
            clock.SetupGet(x => x.UtcDateTime).Returns(DateTime.UtcNow);
            return new TicketAdministrationService(DbContext, currentUser, clock.Object, outbox);
        }

        public async Task<Ticket> SeedTicketAsync()
        {
            var ticket = new Ticket
            {
                TenantId = currentUser.TenantId,
                TicketNumber = "TKT-2026-0999",
                Subject = "Seed ticket",
                TicketType = TicketType.Support,
                Channel = TicketChannelType.Web,
                Priority = PriorityType.High,
                Status = TicketStatusType.New,
                OpenedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                UpdatedBy = "tester"
            };

            DbContext.Tickets.Add(ticket);
            await DbContext.SaveChangesAsync();
            return ticket;
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
