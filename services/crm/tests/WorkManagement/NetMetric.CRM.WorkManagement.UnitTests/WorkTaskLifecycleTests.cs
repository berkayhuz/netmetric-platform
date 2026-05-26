using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.AssignWorkTaskOwner;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CompleteWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.DeleteWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.ReopenWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTask;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskDueDate;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.UpdateWorkTaskReminder;
using NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTaskById;
using NetMetric.CRM.WorkManagement.Application.Queries.Tasks.GetWorkTasks;
using NetMetric.CRM.WorkManagement.Domain.Entities;
using NetMetric.CRM.WorkManagement.Domain.Enums;
using NetMetric.CRM.WorkManagement.Infrastructure.Persistence;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkManagement.UnitTests;

public sealed class WorkTaskLifecycleTests
{
    [Fact]
    public async Task List_And_Detail_Should_Respect_Tenant_Isolation()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantA);

        fixture.Db.Tasks.Add(new WorkTask("A", string.Empty, null, DateTime.UtcNow.AddDays(1), 3) { TenantId = tenantA });
        await fixture.Db.SaveChangesAsync();
        await using (var foreignTenantDb = fixture.CreateTenantDbContext(tenantB))
        {
            foreignTenantDb.Tasks.Add(new WorkTask("B", string.Empty, null, DateTime.UtcNow.AddDays(1), 3) { TenantId = tenantB });
            await foreignTenantDb.SaveChangesAsync();
        }

        var list = await new GetWorkTasksQueryHandler(fixture.Db)
            .Handle(new GetWorkTasksQuery(null, null, null, null, null, 1, 20, "dueAtUtc", "asc"), CancellationToken.None);

        list.TotalCount.Should().Be(1);
        var visible = list.Items.Single();

        var detail = await new GetWorkTaskByIdQueryHandler(fixture.Db)
            .Handle(new GetWorkTaskByIdQuery(visible.Id), CancellationToken.None);
        var foreign = await new GetWorkTaskByIdQueryHandler(fixture.Db)
            .Handle(new GetWorkTaskByIdQuery(fixture.Db.Tasks.IgnoreQueryFilters().Single(x => x.TenantId == tenantB).Id), CancellationToken.None);

        detail.Should().NotBeNull();
        foreign.Should().BeNull();
    }

    [Fact]
    public async Task Lifecycle_Mutations_Should_Succeed_For_Visible_Task()
    {
        var tenant = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenant);
        var task = new WorkTask("Initial", "Desc", null, DateTime.UtcNow.AddDays(2), 2);
        fixture.Db.Tasks.Add(task);
        await fixture.Db.SaveChangesAsync();

        var updated = await new UpdateWorkTaskCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskCommand(task.Id, "Updated", "Updated desc", 4), CancellationToken.None);
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("Updated");

        var ownerId = Guid.NewGuid();
        var ownerUpdated = await new AssignWorkTaskOwnerCommandHandler(fixture.Db)
            .Handle(new AssignWorkTaskOwnerCommand(task.Id, ownerId), CancellationToken.None);
        ownerUpdated!.OwnerUserId.Should().Be(ownerId);

        var dueUpdated = await new UpdateWorkTaskDueDateCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskDueDateCommand(task.Id, DateTime.UtcNow.AddDays(4)), CancellationToken.None);
        dueUpdated.Should().NotBeNull();

        var reminderUpdated = await new UpdateWorkTaskReminderCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskReminderCommand(task.Id, DateTime.UtcNow.AddDays(3)), CancellationToken.None);
        reminderUpdated!.ReminderAtUtc.Should().NotBeNull();

        var reminderCleared = await new UpdateWorkTaskReminderCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskReminderCommand(task.Id, null), CancellationToken.None);
        reminderCleared!.ReminderAtUtc.Should().BeNull();

        var completed = await new CompleteWorkTaskCommandHandler(fixture.Db)
            .Handle(new CompleteWorkTaskCommand(task.Id, Guid.NewGuid(), "done"), CancellationToken.None);
        completed!.Status.Should().Be(nameof(WorkItemStatus.Completed));

        var reopened = await new ReopenWorkTaskCommandHandler(fixture.Db)
            .Handle(new ReopenWorkTaskCommand(task.Id), CancellationToken.None);
        reopened!.Status.Should().Be(nameof(WorkItemStatus.InProgress));

        var deleted = await new DeleteWorkTaskCommandHandler(fixture.Db)
            .Handle(new DeleteWorkTaskCommand(task.Id), CancellationToken.None);
        deleted.Should().BeTrue();

        var stillVisible = await fixture.Db.Tasks.FirstOrDefaultAsync(x => x.Id == task.Id);
        stillVisible.Should().BeNull();

        var physical = await fixture.Db.Tasks.IgnoreQueryFilters().SingleAsync(x => x.Id == task.Id);
        physical.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task Missing_Task_Should_Return_Null_Or_False()
    {
        await using var fixture = await Fixture.CreateAsync(Guid.NewGuid());
        var missingTaskId = Guid.NewGuid();

        var updated = await new UpdateWorkTaskCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskCommand(missingTaskId, "A", "B", 3), CancellationToken.None);
        var completed = await new CompleteWorkTaskCommandHandler(fixture.Db)
            .Handle(new CompleteWorkTaskCommand(missingTaskId, null, null), CancellationToken.None);
        var reopened = await new ReopenWorkTaskCommandHandler(fixture.Db)
            .Handle(new ReopenWorkTaskCommand(missingTaskId), CancellationToken.None);
        var owner = await new AssignWorkTaskOwnerCommandHandler(fixture.Db)
            .Handle(new AssignWorkTaskOwnerCommand(missingTaskId, Guid.NewGuid()), CancellationToken.None);
        var dueDate = await new UpdateWorkTaskDueDateCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskDueDateCommand(missingTaskId, DateTime.UtcNow.AddDays(1)), CancellationToken.None);
        var reminder = await new UpdateWorkTaskReminderCommandHandler(fixture.Db)
            .Handle(new UpdateWorkTaskReminderCommand(missingTaskId, DateTime.UtcNow.AddHours(1)), CancellationToken.None);
        var deleted = await new DeleteWorkTaskCommandHandler(fixture.Db)
            .Handle(new DeleteWorkTaskCommand(missingTaskId), CancellationToken.None);

        updated.Should().BeNull();
        completed.Should().BeNull();
        reopened.Should().BeNull();
        owner.Should().BeNull();
        dueDate.Should().BeNull();
        reminder.Should().BeNull();
        deleted.Should().BeFalse();
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<WorkManagementDbContext> _options;

        private Fixture(SqliteConnection connection, DbContextOptions<WorkManagementDbContext> options, WorkManagementDbContext db)
        {
            _connection = connection;
            _options = options;
            Db = db;
        }

        public WorkManagementDbContext Db { get; }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<WorkManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = CreateDbContext(options, tenantId);

            await db.Database.EnsureCreatedAsync();
            return new Fixture(connection, options, db);
        }

        public WorkManagementDbContext CreateTenantDbContext(Guid tenantId)
            => CreateDbContext(_options, tenantId);

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class TestTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private static WorkManagementDbContext CreateDbContext(DbContextOptions<WorkManagementDbContext> options, Guid tenantId)
        => new(
            options,
            new TestTenantProvider(tenantId),
            new TenantIsolationSaveChangesInterceptor(tenantProvider: new TestTenantProvider(tenantId)),
            new AuditSaveChangesInterceptor(),
            new SoftDeleteSaveChangesInterceptor());
}
