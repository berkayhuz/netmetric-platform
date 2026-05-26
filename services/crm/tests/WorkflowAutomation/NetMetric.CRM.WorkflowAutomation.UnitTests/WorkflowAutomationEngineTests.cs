// <copyright file="WorkflowAutomationEngineTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.UnitTests;

public sealed class WorkflowAutomationEngineTests
{
    [Fact]
    public void Condition_evaluator_matches_nested_conditions_and_not_equals()
    {
        var evaluator = new WorkflowConditionEvaluator();

        var result = evaluator.Evaluate(
            """
            {
              "operator": "and",
              "conditions": [
                { "field": "amount", "comparison": "gte", "value": 1000 },
                { "field": "status", "comparison": "notEquals", "value": "closed" },
                { "operator": "or", "conditions": [
                  { "field": "owner.team", "comparison": "eq", "value": "enterprise" },
                  { "field": "priority", "comparison": "eq", "value": "high" }
                ]}
              ]
            }
            """,
            """{"amount":1500,"status":"open","owner":{"team":"enterprise"},"priority":"low"}""");

        result.Matched.Should().BeTrue();
        result.FailureReasons.Should().BeEmpty();
    }

    [Fact]
    public void Trigger_evaluator_matches_active_rule_with_payload_field()
    {
        var tenantId = Guid.NewGuid();
        var rule = CreateRule(tenantId, triggerDefinitionJson: """{"event":"entity.updated","payloadField":"amount","entityTypes":["deal"]}""");
        var evaluator = new WorkflowTriggerEvaluator();

        var matched = evaluator.IsMatch(rule, new WorkflowRuleExecutionRequest(
            tenantId,
            "entity.updated",
            "deal",
            Guid.NewGuid(),
            """{"amount":25}"""));

        matched.Should().BeTrue();
    }

    [Fact]
    public async Task Action_dispatcher_dry_run_plans_actions_after_permission_check()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        var dispatcher = fixture.CreateDispatcher();
        var rule = CreateRule(tenantId, actionDefinitionJson: """[{"type":"audit.log","requiredPermission":"workflow.rules.manage"}]""");

        var result = await dispatcher.DispatchAsync(new WorkflowActionDispatchContext(
            rule,
            tenantId,
            Guid.Empty,
            "entity.updated",
            "deal",
            Guid.NewGuid(),
            """{"amount":10}""",
            rule.ActionDefinitionJson,
            "corr-1",
            true,
            """["workflow.rules.manage"]"""), CancellationToken.None);

        result.ExecutedActions.Should().Be(0);
        result.Actions.Should().ContainSingle(action => action.Status == "planned" && action.RequiresPermission);
    }

    [Fact]
    public async Task Dry_run_persists_simulation_log_without_executing_actions()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        await fixture.SeedRuleAsync(CreateRule(tenantId, actionDefinitionJson: """[{"type":"audit.log"}]"""));
        var engine = fixture.CreateEngine(fixture.CreateDispatcher());

        var result = await engine.DryRunAsync(new WorkflowRuleExecutionRequest(
            tenantId,
            "entity.updated",
            "deal",
            Guid.NewGuid(),
            """{"amount":10,"apiKey":"secret-value"}"""), CancellationToken.None);

        result.DryRun.Should().BeTrue();
        result.Status.Should().Be(WorkflowExecutionStatuses.Simulated);
        var log = await fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().SingleAsync();
        log.IsDryRun.Should().BeTrue();
        log.TriggerPayloadJson.Should().Contain("[redacted]");
        log.ActionResultJson.Should().Contain("planned");
    }

    [Fact]
    public async Task Execute_is_idempotent_for_completed_execution_with_same_key()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        await fixture.SeedRuleAsync(CreateRule(tenantId, actionDefinitionJson: """[{"type":"audit.log"}]"""));
        var engine = fixture.CreateEngine(fixture.CreateDispatcher());
        var request = new WorkflowRuleExecutionRequest(tenantId, "entity.updated", "deal", Guid.NewGuid(), """{"amount":10}""", "same-key");

        var first = await engine.ExecuteAsync(request, CancellationToken.None);
        var second = await engine.ExecuteAsync(request, CancellationToken.None);

        first.Status.Should().Be(WorkflowExecutionStatuses.Completed);
        second.Rules.Single().Actions.Single().Status.Should().Be(WorkflowExecutionStatuses.IdempotentSkip);
        fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().Count().Should().Be(1);
        fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().Single().Status.Should().Be(WorkflowExecutionStatuses.Completed);
    }

    [Fact]
    public async Task Retryable_action_failure_marks_execution_retrying_with_sanitized_error()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        await fixture.SeedRuleAsync(CreateRule(tenantId, actionDefinitionJson: """[{"type":"audit.log"}]""", maxAttempts: 3));
        var engine = fixture.CreateEngine(new ThrowingDispatcher(new WorkflowTransientException("token=super-secret upstream timeout")));

        var result = await engine.ExecuteAsync(new WorkflowRuleExecutionRequest(
            tenantId,
            "entity.updated",
            "deal",
            Guid.NewGuid(),
            """{"amount":10}"""), CancellationToken.None);

        result.Status.Should().Be(WorkflowExecutionStatuses.Completed);
        var log = await fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().SingleAsync();
        log.Status.Should().Be(WorkflowExecutionStatuses.Retrying);
        log.ErrorMessage.Should().Contain("[redacted]");
        log.NextAttemptAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Loop_prevention_skips_recent_recursive_execution()
    {
        var tenantId = Guid.NewGuid();
        var entityId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        var rule = CreateRule(tenantId, actionDefinitionJson: """[{"type":"audit.log"}]""");
        await fixture.SeedRuleAsync(rule);
        var fingerprint = WorkflowExecutionPolicy.ComputeLoopFingerprint(tenantId, rule, "entity.updated", "deal", entityId);
        var previous = RuleExecutionLog.Queue(tenantId, rule.Id, rule.Version, rule.Name, "entity.updated", "deal", entityId, "previous-key", "corr-prev", fingerprint, 0, 3, DateTime.UtcNow.AddSeconds(-1), "{}", "[]", null);
        previous.TryAcquire("worker", DateTime.UtcNow, TimeSpan.FromMinutes(1));
        previous.MarkCompleted("{}", "[]", DateTime.UtcNow);
        fixture.Context.RuleExecutionLogs.Add(previous);
        await fixture.Context.SaveChangesAsync();
        var engine = fixture.CreateEngine(fixture.CreateDispatcher());

        var result = await engine.ExecuteAsync(new WorkflowRuleExecutionRequest(
            tenantId,
            "entity.updated",
            "deal",
            entityId,
            """{"amount":20}""",
            "new-key"), CancellationToken.None);

        result.Rules.Single().LoopPrevented.Should().BeTrue();
        fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().Count(x => x.Status == WorkflowExecutionStatuses.LoopPrevented).Should().Be(1);
    }

    [Fact]
    public async Task Tenant_query_filter_limits_execution_logs_to_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await using var seed = await WorkflowFixture.CreateAsync(null);
        seed.Context.RuleExecutionLogs.Add(CreateQueuedLog(tenantId, Guid.NewGuid(), "tenant-key"));
        seed.Context.RuleExecutionLogs.Add(CreateQueuedLog(otherTenantId, Guid.NewGuid(), "other-key"));
        await seed.Context.SaveChangesAsync();

        await using var tenantFixture = WorkflowFixture.Create(seed.Connection, tenantId);

        tenantFixture.Context.RuleExecutionLogs.Count().Should().Be(1);
        tenantFixture.Context.RuleExecutionLogs.Single().TenantId.Should().Be(tenantId);
    }

    [Fact]
    public async Task Permission_guard_rejects_action_without_user_or_snapshot_permission()
    {
        var guard = new WorkflowActionPermissionGuard(new TestCurrentUser(Guid.NewGuid(), Guid.NewGuid(), []));

        var act = () => guard.AuthorizeAsync(new WorkflowActionPermissionContext("webhook.post", null, "[]", false), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*workflow.webhooks.manage*");
    }

    [Fact]
    public async Task Scheduled_worker_processes_due_execution_once()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await WorkflowFixture.CreateAsync(tenantId);
        var rule = CreateRule(tenantId);
        await fixture.SeedRuleAsync(rule);
        var due = CreateQueuedLog(tenantId, rule.Id, "due-key", DateTime.UtcNow.AddSeconds(-5));
        fixture.Context.RuleExecutionLogs.Add(due);
        await fixture.Context.SaveChangesAsync();
        var processor = new WorkflowExecutionProcessor(
            fixture.Context,
            new CompletingRuleEngine(fixture.Context),
            Options.Create(new WorkflowAutomationOptions { BatchSize = 10, LeaseSeconds = 30 }),
            NullLogger<WorkflowExecutionProcessor>.Instance);

        var processed = await processor.ProcessDueExecutionsAsync(CancellationToken.None);

        processed.Should().Be(1);
        var log = await fixture.Context.RuleExecutionLogs.IgnoreQueryFilters().SingleAsync();
        log.Status.Should().Be(WorkflowExecutionStatuses.Completed);
        log.AttemptNumber.Should().Be(1);
    }

    [Fact]
    public void Execution_log_tracks_retry_and_dead_letter_state()
    {
        var log = CreateQueuedLog(Guid.NewGuid(), Guid.NewGuid(), "state-key");

        log.TryAcquire("worker", DateTime.UtcNow, TimeSpan.FromSeconds(30)).Should().BeTrue();
        log.MarkRetry(DateTime.UtcNow.AddMinutes(1), "transient", "timeout", "upstream timeout");
        log.Status.Should().Be(WorkflowExecutionStatuses.Retrying);
        log.LockedBy.Should().BeNull();

        log.TryAcquire("worker", DateTime.UtcNow.AddMinutes(2), TimeSpan.FromSeconds(30)).Should().BeTrue();
        log.MoveToDeadLetter(DateTime.UtcNow, "transient", "timeout", "upstream timeout");

        log.Status.Should().Be(WorkflowExecutionStatuses.DeadLettered);
        log.DeadLetteredAtUtc.Should().NotBeNull();
    }

    private static AutomationRule CreateRule(
        Guid tenantId,
        string triggerDefinitionJson = """{"event":"entity.updated"}""",
        string conditionDefinitionJson = "{}",
        string actionDefinitionJson = """[{"type":"audit.log"}]""",
        int maxAttempts = 3)
    {
        var rule = AutomationRule.Create(
            "High value deal",
            "entity.updated",
            "deal",
            triggerDefinitionJson,
            conditionDefinitionJson,
            actionDefinitionJson,
            maxAttempts: maxAttempts,
            isActive: true);
        rule.TenantId = tenantId;
        return rule;
    }

    private static RuleExecutionLog CreateQueuedLog(Guid tenantId, Guid ruleId, string key, DateTime? scheduledAtUtc = null)
        => RuleExecutionLog.Queue(
            tenantId,
            ruleId,
            1,
            "Rule",
            "entity.updated",
            "deal",
            Guid.NewGuid(),
            key,
            $"corr-{key}",
            $"{tenantId:N}:{ruleId:N}:entity.updated:deal",
            0,
            3,
            scheduledAtUtc ?? DateTime.UtcNow.AddSeconds(-1),
            "{}",
            "[]",
            null);

    private sealed class WorkflowFixture : IAsyncDisposable
    {
        private readonly bool _ownsConnection;
        private readonly TestTenantContext _tenantContext;
        private readonly TestCurrentUser _currentUser;

        private WorkflowFixture(SqliteConnection connection, WorkflowAutomationDbContext context, TestTenantContext tenantContext, TestCurrentUser currentUser, bool ownsConnection)
        {
            Connection = connection;
            Context = context;
            _tenantContext = tenantContext;
            _currentUser = currentUser;
            _ownsConnection = ownsConnection;
        }

        public SqliteConnection Connection { get; }
        public WorkflowAutomationDbContext Context { get; }

        public static async Task<WorkflowFixture> CreateAsync(Guid? tenantId)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var fixture = Create(connection, tenantId, ownsConnection: true);
            await fixture.Context.Database.EnsureCreatedAsync();
            return fixture;
        }

        public static WorkflowFixture Create(SqliteConnection connection, Guid? tenantId, bool ownsConnection = false)
        {
            var tenantContext = new TestTenantContext(tenantId);
            var currentUser = tenantId.HasValue
                ? new TestCurrentUser(Guid.NewGuid(), tenantId.Value, ["workflow.rules.manage", "workflow.webhooks.manage"])
                : new TestCurrentUser(Guid.Empty, Guid.Empty, []);
            var options = new DbContextOptionsBuilder<WorkflowAutomationDbContext>()
                .UseSqlite(connection)
                .Options;
            var context = new WorkflowAutomationDbContext(
                options,
                tenantContext,
                new AuditSaveChangesInterceptor(currentUser),
                new SoftDeleteSaveChangesInterceptor(currentUser),
                new TenantIsolationSaveChangesInterceptor(tenantContext, null, currentUser));
            return new WorkflowFixture(connection, context, tenantContext, currentUser, ownsConnection);
        }

        public async Task SeedRuleAsync(AutomationRule rule)
        {
            Context.AutomationRules.Add(rule);
            await Context.SaveChangesAsync();
        }

        public WorkflowActionDispatcher CreateDispatcher()
        {
            var workflowOptions = Options.Create(new WorkflowAutomationOptions());
            return new WorkflowActionDispatcher(
                Context,
                new WorkflowActionPermissionGuard(_currentUser),
                new WorkflowPayloadRedactor(),
                new HttpClient(),
                new WebhookOutboundRequestValidator(workflowOptions, new SystemWebhookDnsResolver()),
                NullLogger<WorkflowActionDispatcher>.Instance,
                workflowOptions);
        }

        public WorkflowRuleEngine CreateEngine(IWorkflowActionDispatcher dispatcher)
            => new(
                Context,
                new WorkflowTriggerEvaluator(),
                new WorkflowConditionEvaluator(),
                dispatcher,
                new WorkflowPayloadRedactor(),
                _currentUser,
                Options.Create(new WorkflowAutomationOptions { EngineEnabled = true, MaxAttempts = 3, BaseRetryDelaySeconds = 1, MaxRetryDelaySeconds = 2, LeaseSeconds = 30 }),
                NullLogger<WorkflowRuleEngine>.Instance);

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            if (_ownsConnection)
            {
                await Connection.DisposeAsync();
            }
        }
    }

    private sealed class TestTenantContext(Guid? tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private sealed class TestCurrentUser(Guid userId, Guid tenantId, IReadOnlyCollection<string> permissions) : ICurrentUserService
    {
        public Guid UserId { get; } = userId;
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => UserId != Guid.Empty;
        public string? UserName => "workflow-test";
        public string? Email => "workflow-test@example.com";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions { get; } = permissions;
        public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class ThrowingDispatcher(Exception exception) : IWorkflowActionDispatcher
    {
        public Task<WorkflowActionDispatchResult> DispatchAsync(WorkflowActionDispatchContext context, CancellationToken cancellationToken)
            => Task.FromException<WorkflowActionDispatchResult>(exception);
    }

    private sealed class CompletingRuleEngine(WorkflowAutomationDbContext context) : IWorkflowRuleEngine
    {
        public Task<WorkflowRuleExecutionResultDto> ExecuteAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public Task<WorkflowRuleExecutionResultDto> DryRunAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        public async Task<WorkflowRuleExecutionResultDto> ExecuteQueuedAsync(Guid executionLogId, CancellationToken cancellationToken)
        {
            var log = await context.RuleExecutionLogs.IgnoreQueryFilters().SingleAsync(x => x.Id == executionLogId, cancellationToken);
            log.MarkCompleted("{}", "[]", DateTime.UtcNow);
            await context.SaveChangesAsync(cancellationToken);
            return new WorkflowRuleExecutionResultDto(executionLogId, false, 1, 0, WorkflowExecutionStatuses.Completed, []);
        }
    }
}
