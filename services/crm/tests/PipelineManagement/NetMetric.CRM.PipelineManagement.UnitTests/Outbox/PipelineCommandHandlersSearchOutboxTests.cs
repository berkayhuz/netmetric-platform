// <copyright file="PipelineCommandHandlersSearchOutboxTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Application.Handlers;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Tenancy;

namespace NetMetric.CRM.PipelineManagement.UnitTests.Outbox;

public sealed class PipelineCommandHandlersSearchOutboxTests
{
    [Fact]
    public async Task CreatePipelineCommandHandler_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var outbox = new Mock<IPipelineManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueuePipelineCreatedAsync(It.IsAny<Pipeline>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new CreatePipelineCommandHandler(fixture.DbContext, outbox.Object);

        var result = await sut.Handle(
            new CreatePipelineCommand(
                "Search Pipeline Create",
                null,
                false,
                10,
                [new CreatePipelineStageRequest("Stage A", null, 1, 20m, false, false)]),
            CancellationToken.None);

        result.Name.Should().Be("Search Pipeline Create");
        outbox.Verify(x => x.EnqueuePipelineCreatedAsync(It.Is<Pipeline>(p => p.Id == result.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePipelineCommandHandler_Should_Enqueue_Search_Index_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedPipelineAsync();
        var outbox = new Mock<IPipelineManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueuePipelineUpdatedAsync(It.IsAny<Pipeline>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new UpdatePipelineCommandHandler(fixture.DbContext, fixture.CurrentUser, outbox.Object);

        var result = await sut.Handle(
            new UpdatePipelineCommand(
                seeded.Id,
                "Search Pipeline Updated",
                null,
                false,
                11,
                [new UpdatePipelineStageRequest(seeded.Stages.Single().Id, "Stage A+", null, 1, 30m, false, false)],
                Convert.ToBase64String(seeded.RowVersion)),
            CancellationToken.None);

        result.Name.Should().Be("Search Pipeline Updated");
        outbox.Verify(x => x.EnqueuePipelineUpdatedAsync(It.Is<Pipeline>(p => p.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeletePipelineCommandHandler_Should_Enqueue_Search_Delete_Event()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var seeded = await fixture.SeedPipelineAsync();
        var outbox = new Mock<IPipelineManagementOutbox>(MockBehavior.Strict);
        outbox.Setup(x => x.EnqueuePipelineDeletedAsync(It.IsAny<Pipeline>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        var sut = new DeletePipelineCommandHandler(fixture.DbContext, fixture.CurrentUser, outbox.Object);

        await sut.Handle(new DeletePipelineCommand(seeded.Id), CancellationToken.None);

        outbox.Verify(x => x.EnqueuePipelineDeletedAsync(It.Is<Pipeline>(p => p.Id == seeded.Id), It.IsAny<CancellationToken>()), Times.Once);
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

            var dbContext = new PipelineManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(connection, dbContext, new FixedCurrentUser(tenantId));
        }

        public async Task<Pipeline> SeedPipelineAsync()
        {
            var pipeline = new Pipeline
            {
                TenantId = CurrentUser.TenantId,
                Name = "Seed Pipeline",
                Description = null,
                IsDefault = false,
                DisplayOrder = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = "tester",
                UpdatedBy = "tester",
                Stages =
                [
                    new PipelineStage
                    {
                        TenantId = CurrentUser.TenantId,
                        Name = "Stage A",
                        Description = null,
                        DisplayOrder = 1,
                        Probability = 20m,
                        IsWinStage = false,
                        IsLostStage = false
                    }
                ]
            };

            DbContext.Pipelines.Add(pipeline);
            await DbContext.SaveChangesAsync();
            await DbContext.Entry(pipeline).Collection(x => x.Stages).LoadAsync();
            return pipeline;
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
