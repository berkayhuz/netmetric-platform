// <copyright file="CustomerManagementTrashRetentionProcessorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using MediatR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Commands.Trash;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.TrashRetention;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.UnitTests.TrashRetention;

public sealed class CustomerManagementTrashRetentionProcessorTests
{
    [Fact]
    public async Task ProcessCycleAsync_ShouldNotCallMediator_WhenDisabled()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = false, BatchSize = 10, MaxTenantsPerRun = 10 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(0);
        result.PurgedItems.Should().Be(0);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessCycleAsync_ShouldCallCommandPerTenant_WithConfiguredBatchSize()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await fixture.SeedTrashAsync(
            TrashItem(tenantA, DateTime.UtcNow.AddMinutes(-5)),
            TrashItem(tenantB, DateTime.UtcNow.AddMinutes(-10)));

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IRequest<int> request, CancellationToken _) =>
            {
                var command = (PurgeExpiredTrashItemsCommand)request;
                command.BatchSize.Should().Be(25);
                command.TenantId.Should().NotBeNull();
                return 3;
            });

        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = true, BatchSize = 25, MaxTenantsPerRun = 10 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(2);
        result.PurgedItems.Should().Be(6);
        mediator.Verify(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessCycleAsync_ShouldRespectMaxTenantsPerRun()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        await fixture.SeedTrashAsync(
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5)),
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-6)),
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-7)));

        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = true, BatchSize = 10, MaxTenantsPerRun = 2 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(2);
        mediator.Verify(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessCycleAsync_ShouldSkipNonExpiredAndUnsupportedEntityTypes()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        await fixture.SeedTrashAsync(
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(5)),
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5), status: CrmTrashStatuses.Restored),
            TrashItem(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(-5), entityType: "unsupported"));

        var mediator = new Mock<IMediator>(MockBehavior.Strict);
        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = true, BatchSize = 10, MaxTenantsPerRun = 10 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(0);
        result.PurgedItems.Should().Be(0);
        mediator.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task ProcessCycleAsync_ShouldEnumerateTenantsFromSupportedEntityTypes()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var tenantC = Guid.NewGuid();
        var tenantD = Guid.NewGuid();
        var tenantE = Guid.NewGuid();
        var tenantF = Guid.NewGuid();
        var tenantG = Guid.NewGuid();
        var tenantH = Guid.NewGuid();
        var tenantI = Guid.NewGuid();

        await fixture.SeedTrashAsync(
            TrashItem(tenantA, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Contact),
            TrashItem(tenantB, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Customer),
            TrashItem(tenantC, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Company),
            TrashItem(tenantD, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Lead),
            TrashItem(tenantE, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Deal),
            TrashItem(tenantF, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Opportunity),
            TrashItem(tenantG, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Quote),
            TrashItem(tenantH, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.Ticket),
            TrashItem(tenantI, DateTime.UtcNow.AddMinutes(-5), entityType: CrmTrashEntityTypes.ProductCatalogItem));

        var mediator = new Mock<IMediator>();
        mediator.Setup(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = true, BatchSize = 10, MaxTenantsPerRun = 10 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(9);
        result.PurgedItems.Should().Be(9);
        mediator.Verify(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(9));
    }

    [Fact]
    public async Task ProcessCycleAsync_ShouldContinueWhenTenantCommandFails()
    {
        await using var fixture = await ProcessorFixture.CreateAsync();
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await fixture.SeedTrashAsync(
            TrashItem(tenantA, DateTime.UtcNow.AddMinutes(-5)),
            TrashItem(tenantB, DateTime.UtcNow.AddMinutes(-10)));

        var mediator = new Mock<IMediator>();
        mediator
            .Setup(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IRequest<int> request, CancellationToken _) =>
            {
                var command = (PurgeExpiredTrashItemsCommand)request;
                if (command.TenantId == tenantA)
                {
                    throw new InvalidOperationException("boom");
                }

                return 2;
            });

        var processor = fixture.CreateProcessor(
            mediator.Object,
            new CustomerManagementTrashRetentionOptions { Enabled = true, BatchSize = 10, MaxTenantsPerRun = 10 });

        var result = await processor.ProcessCycleAsync(CancellationToken.None);

        result.ProcessedTenants.Should().Be(1);
        result.PurgedItems.Should().Be(2);
        mediator.Verify(x => x.Send(It.IsAny<PurgeExpiredTrashItemsCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    private static GlobalTrashItem TrashItem(
        Guid tenantId,
        DateTime expiresAtUtc,
        string status = CrmTrashStatuses.Active,
        string entityType = CrmTrashEntityTypes.Contact)
        => new()
        {
            TenantId = tenantId,
            EntityType = entityType,
            EntityId = Guid.NewGuid(),
            DisplayName = "Expired Contact",
            SourceModule = "contacts",
            DeletedAtUtc = DateTime.UtcNow.AddDays(-8),
            ExpiresAtUtc = expiresAtUtc,
            Status = status,
        };

    private sealed class ProcessorFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private ProcessorFixture(SqliteConnection connection, CustomerManagementDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public CustomerManagementDbContext DbContext { get; }

        public static async Task<ProcessorFixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new CustomerManagementDbContext(options, new NullTenantProvider());
            await dbContext.Database.EnsureCreatedAsync();
            return new ProcessorFixture(connection, dbContext);
        }

        public CustomerManagementTrashRetentionProcessor CreateProcessor(
            IMediator mediator,
            CustomerManagementTrashRetentionOptions configured) =>
            new(
                DbContext,
                mediator,
                Options.Create(configured),
                NullLogger<CustomerManagementTrashRetentionProcessor>.Instance);

        public async Task SeedTrashAsync(params GlobalTrashItem[] items)
        {
            await DbContext.GlobalTrashItems.AddRangeAsync(items);
            await DbContext.SaveChangesAsync(CancellationToken.None);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class NullTenantProvider : ITenantProvider
    {
        public Guid? TenantId => null;
    }
}
