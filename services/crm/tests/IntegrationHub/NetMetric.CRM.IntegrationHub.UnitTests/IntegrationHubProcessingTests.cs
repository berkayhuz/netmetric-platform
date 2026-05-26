// <copyright file="IntegrationHubProcessingTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Connectors;
using NetMetric.CRM.IntegrationHub.Application.Commands.CancelIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.ReplayIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.ScheduleIntegrationJob;
using NetMetric.CRM.IntegrationHub.Application.Commands.ValidateWebhookDelivery;
using NetMetric.CRM.IntegrationHub.Application.Queries.GetConnectorHealth;
using NetMetric.CRM.IntegrationHub.Application.Queries.ListIntegrationJobs;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;
using NetMetric.CRM.IntegrationHub.Infrastructure.Processing;
using NetMetric.CRM.IntegrationHub.Infrastructure.Webhooks;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.UnitTests;

public sealed class IntegrationHubProcessingTests
{
    [Fact]
    public async Task Worker_completes_due_job_and_records_execution_log()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(null);
        await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "job-success");
        var connector = new RecordingConnector("internal-crm");
        var processor = fixture.CreateProcessor(connector);

        var processed = await processor.ProcessDueJobsAsync(CancellationToken.None);

        processed.Should().Be(1);
        connector.ExecutionCount.Should().Be(1);
        var job = await fixture.Context.IntegrationJobs.IgnoreQueryFilters().SingleAsync();
        job.Status.Should().Be(IntegrationJobStatuses.Completed);
        job.AttemptCount.Should().Be(1);
        fixture.Context.IntegrationJobExecutionLogs.IgnoreQueryFilters().Count().Should().Be(1);
    }

    [Fact]
    public async Task Worker_retries_retryable_connector_failure_with_backoff()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(null);
        await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "job-retry");
        var processor = fixture.CreateProcessor(new ThrowingConnector("internal-crm", new IntegrationTransientException("token=[secret] upstream timeout")), maxAttempts: 3);

        await processor.ProcessDueJobsAsync(CancellationToken.None);

        var job = await fixture.Context.IntegrationJobs.IgnoreQueryFilters().SingleAsync();
        job.Status.Should().Be(IntegrationJobStatuses.Retrying);
        job.AttemptCount.Should().Be(1);
        job.NextAttemptAtUtc.Should().BeAfter(DateTime.UtcNow.AddSeconds(-1));
        job.LastErrorMessage.Should().Contain("[redacted]");
        fixture.Context.IntegrationDeadLetters.IgnoreQueryFilters().Should().BeEmpty();
    }

    [Fact]
    public async Task Worker_moves_exhausted_job_to_dead_letter_queue()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(null);
        await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "job-dlq", maxAttempts: 1);
        var processor = fixture.CreateProcessor(new ThrowingConnector("internal-crm", new IntegrationTransientException("upstream timeout")), maxAttempts: 1);

        await processor.ProcessDueJobsAsync(CancellationToken.None);

        var job = await fixture.Context.IntegrationJobs.IgnoreQueryFilters().SingleAsync();
        job.Status.Should().Be(IntegrationJobStatuses.DeadLettered);
        fixture.Context.IntegrationDeadLetters.IgnoreQueryFilters().Count().Should().Be(1);
    }

    [Fact]
    public async Task Schedule_job_is_idempotent_for_same_tenant_provider_and_payload()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(tenantId);
        var handler = new ScheduleIntegrationJobCommandHandler(fixture.Context, new TestTenantContext(tenantId));
        var command = new ScheduleIntegrationJobCommand(tenantId, "sync-accounts", "outbound", """{"entity":"account"}""", DateTime.UtcNow, "internal-crm");

        var first = await handler.Handle(command, CancellationToken.None);
        var second = await handler.Handle(command, CancellationToken.None);

        second.Should().Be(first);
        fixture.Context.IntegrationJobs.Count().Should().Be(1);
    }

    [Fact]
    public async Task List_jobs_query_is_tenant_isolated()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(tenantId);
        await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "tenant-job");
        await fixture.SeedConnectionAndJobAsync(otherTenantId, "internal-crm", idempotencyKey: "other-job");

        var result = await new ListIntegrationJobsQueryHandler(fixture.Context, new TestTenantContext(tenantId))
            .Handle(new ListIntegrationJobsQuery(tenantId, null, null, 1, 20), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Id.Should().Be(fixture.Context.IntegrationJobs.Single().Id);
    }

    [Fact]
    public async Task Cancel_command_prevents_queued_job_from_executing()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(tenantId);
        var job = await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "job-cancel");

        await new CancelIntegrationJobCommandHandler(fixture.Context, new TestTenantContext(tenantId))
            .Handle(new CancelIntegrationJobCommand(tenantId, job.Id, "operator request"), CancellationToken.None);
        var connector = new RecordingConnector("internal-crm");
        await fixture.CreateProcessor(connector).ProcessDueJobsAsync(CancellationToken.None);

        connector.ExecutionCount.Should().Be(0);
        var canceled = await fixture.Context.IntegrationJobs.IgnoreQueryFilters().SingleAsync();
        canceled.Status.Should().Be(IntegrationJobStatuses.Canceled);
    }

    [Fact]
    public async Task Replay_rejects_cross_tenant_request()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(otherTenantId);
        var job = await fixture.SeedConnectionAndJobAsync(tenantId, "internal-crm", idempotencyKey: "job-replay");
        job.MoveToDeadLetter(DateTime.UtcNow, "permanent", "failed", "Failed");
        await fixture.Context.SaveChangesAsync();

        var act = () => new ReplayIntegrationJobCommandHandler(fixture.Context, new TestTenantContext(otherTenantId))
            .Handle(new ReplayIntegrationJobCommand(tenantId, job.Id), CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Webhook_signature_validation_accepts_signed_payload_and_blocks_replay()
    {
        var tenantId = Guid.NewGuid();
        var secret = "super-secret-value";
        var providerKey = "internal-crm";
        await using var fixture = await IntegrationHubFixture.CreateAsync(tenantId);
        fixture.Context.IntegrationConnections.Add(new IntegrationConnection(tenantId, providerKey, "Internal CRM", "internal", $$"""{"webhookSecret":"{{secret}}"}"""));
        await fixture.Context.SaveChangesAsync();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var payload = """{"event":"customer.updated"}""";
        var signature = ComputeWebhookSignature(secret, timestamp, payload);
        var handler = new ValidateWebhookDeliveryCommandHandler(fixture.Context, new TestTenantContext(tenantId), new HmacIntegrationWebhookSecurityService());

        var accepted = await handler.Handle(new ValidateWebhookDeliveryCommand(tenantId, providerKey, "evt-1", timestamp, signature, payload), CancellationToken.None);
        var duplicate = await handler.Handle(new ValidateWebhookDeliveryCommand(tenantId, providerKey, "evt-1", timestamp, signature, payload), CancellationToken.None);

        accepted.Accepted.Should().BeTrue();
        duplicate.Duplicate.Should().BeTrue();
        fixture.Context.WebhookDeliveries.Count().Should().Be(1);
    }

    [Fact]
    public async Task Connector_health_query_updates_connection_health_without_exposing_settings()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await IntegrationHubFixture.CreateAsync(tenantId);
        fixture.Context.IntegrationConnections.Add(new IntegrationConnection(tenantId, "internal-crm", "Internal CRM", "internal", """{"webhookSecret":"secret"}"""));
        await fixture.Context.SaveChangesAsync();

        var result = await new GetConnectorHealthQueryHandler(
                fixture.Context,
                new TestTenantContext(tenantId),
                new TestConnectorRegistry(new RecordingConnector("internal-crm")))
            .Handle(new GetConnectorHealthQuery(tenantId), CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().HealthStatus.Should().Be(IntegrationConnectorHealthStates.Healthy);
        result.Single().SecretVersion.Should().Be(1);
    }

    private static string ComputeWebhookSignature(string secret, string timestamp, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var digest = hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}"));
        return "sha256=" + Convert.ToHexString(digest).ToLowerInvariant();
    }

    private sealed class IntegrationHubFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private IntegrationHubFixture(SqliteConnection connection, IntegrationHubDbContext context)
        {
            _connection = connection;
            Context = context;
        }

        public IntegrationHubDbContext Context { get; }

        public static async Task<IntegrationHubFixture> CreateAsync(Guid? tenantId)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<IntegrationHubDbContext>()
                .UseSqlite(connection)
                .Options;
            var dataProtection = new PassthroughDataProtectionProvider();
            var context = new IntegrationHubDbContext(options, new TestTenantContext(tenantId), dataProtection);
            await context.Database.EnsureCreatedAsync();
            return new IntegrationHubFixture(connection, context);
        }

        public async Task<IntegrationJob> SeedConnectionAndJobAsync(Guid tenantId, string providerKey, string idempotencyKey, int maxAttempts = 3)
        {
            if (!await Context.IntegrationConnections.IgnoreQueryFilters().AnyAsync(x => x.TenantId == tenantId && x.ProviderKey == providerKey))
            {
                Context.IntegrationConnections.Add(new IntegrationConnection(tenantId, providerKey, "Internal CRM", "internal", """{"webhookSecret":"secret"}"""));
            }

            var job = new IntegrationJob(tenantId, providerKey, "sync-accounts", "outbound", """{"entity":"account"}""", DateTime.UtcNow.AddSeconds(-1), idempotencyKey, maxAttempts, null);
            Context.IntegrationJobs.Add(job);
            await Context.SaveChangesAsync();
            return job;
        }

        public IntegrationJobProcessor CreateProcessor(IIntegrationConnector connector, int maxAttempts = 3)
            => new(
                Context,
                new TestConnectorRegistry(connector),
                Options.Create(new IntegrationJobProcessingOptions
                {
                    Enabled = true,
                    BatchSize = 10,
                    MaxAttempts = maxAttempts,
                    BaseRetryDelaySeconds = 1,
                    MaxRetryDelaySeconds = 2,
                    LeaseSeconds = 30
                }),
                NullLogger<IntegrationJobProcessor>.Instance);

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }

    private sealed class TestTenantContext(Guid? tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private sealed class PassthroughDataProtectionProvider : IDataProtectionProvider, IDataProtector
    {
        public IDataProtector CreateProtector(string purpose) => this;

        public byte[] Protect(byte[] plaintext) => plaintext;

        public byte[] Unprotect(byte[] protectedData) => protectedData;
    }

    private sealed class TestConnectorRegistry(params IIntegrationConnector[] connectors) : IIntegrationConnectorRegistry
    {
        public IIntegrationConnector? Resolve(string providerKey)
            => connectors.FirstOrDefault(x => string.Equals(x.ProviderKey, providerKey, StringComparison.OrdinalIgnoreCase));
    }

    private sealed class RecordingConnector(string providerKey) : IIntegrationConnector
    {
        public string ProviderKey { get; } = providerKey;
        public int ExecutionCount { get; private set; }

        public Task<IntegrationConnectorHealthResult> CheckHealthAsync(IntegrationConnection connection, CancellationToken cancellationToken)
            => Task.FromResult(new IntegrationConnectorHealthResult(IntegrationConnectorHealthStates.Healthy, "Connector adapter is reachable.", DateTime.UtcNow));

        public Task<IntegrationJobExecutionResult> ExecuteAsync(IntegrationJobExecutionContext context, CancellationToken cancellationToken)
        {
            ExecutionCount += 1;
            return Task.FromResult(new IntegrationJobExecutionResult(true, "Connector job completed.", "delta-token"));
        }
    }

    private sealed class ThrowingConnector(string providerKey, Exception exception) : IIntegrationConnector
    {
        public string ProviderKey { get; } = providerKey;

        public Task<IntegrationConnectorHealthResult> CheckHealthAsync(IntegrationConnection connection, CancellationToken cancellationToken)
            => Task.FromResult(new IntegrationConnectorHealthResult(IntegrationConnectorHealthStates.Degraded, "Connector health depends on pending job retry.", DateTime.UtcNow));

        public Task<IntegrationJobExecutionResult> ExecuteAsync(IntegrationJobExecutionContext context, CancellationToken cancellationToken)
            => Task.FromException<IntegrationJobExecutionResult>(exception);
    }
}
