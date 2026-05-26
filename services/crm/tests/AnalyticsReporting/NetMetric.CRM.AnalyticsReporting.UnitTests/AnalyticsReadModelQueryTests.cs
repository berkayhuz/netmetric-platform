// <copyright file="AnalyticsReadModelQueryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetAnalyticsProjectionStatus;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetSalesFunnelSummary;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetTenantReportSummary;
using NetMetric.CRM.AnalyticsReporting.Domain.Entities;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;
using NetMetric.Tenancy;

namespace NetMetric.CRM.AnalyticsReporting.UnitTests;

public sealed class AnalyticsReadModelQueryTests
{
    [Fact]
    public async Task Sales_funnel_query_returns_latest_snapshot_for_current_tenant()
    {
        var tenantId = Guid.NewGuid();
        var otherTenantId = Guid.NewGuid();
        await using var context = await CreateContextAsync(tenantId);

        context.SalesFunnelSnapshots.Add(new AnalyticsSalesFunnelSnapshot(tenantId, 1, 2, 3, 4, 500m, DateTime.UtcNow.AddMinutes(-10)));
        context.SalesFunnelSnapshots.Add(new AnalyticsSalesFunnelSnapshot(tenantId, 5, 6, 7, 8, 900m, DateTime.UtcNow));
        context.SalesFunnelSnapshots.Add(new AnalyticsSalesFunnelSnapshot(otherTenantId, 50, 60, 70, 80, 9000m, DateTime.UtcNow));
        await context.SaveChangesAsync();

        var result = await new GetSalesFunnelSummaryQueryHandler(context)
            .Handle(new GetSalesFunnelSummaryQuery(tenantId), CancellationToken.None);

        result.OpenLeads.Should().Be(5);
        result.QualifiedLeads.Should().Be(6);
        result.OpenOpportunities.Should().Be(7);
        result.WonDeals.Should().Be(8);
        result.PipelineValue.Should().Be(900m);
    }

    [Fact]
    public async Task Tenant_summary_query_returns_empty_collection_when_projection_is_missing()
    {
        var tenantId = Guid.NewGuid();
        await using var context = await CreateContextAsync(tenantId);

        var result = await new GetTenantReportSummaryQueryHandler(context)
            .Handle(new GetTenantReportSummaryQuery(tenantId), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Projection_status_reports_last_success_and_snapshot_freshness()
    {
        var tenantId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        await using var context = await CreateContextAsync(tenantId);

        var run = new AnalyticsProjectionRun("projection-test", now.AddMinutes(-1));
        run.MarkSucceeded(1, now);
        context.ProjectionRuns.Add(run);
        context.TenantSnapshots.Add(new AnalyticsTenantSnapshot(tenantId, "Tenant", 2, 3, 100m, 4, now));
        await context.SaveChangesAsync();

        var result = await new GetAnalyticsProjectionStatusQueryHandler(context)
            .Handle(new GetAnalyticsProjectionStatusQuery(tenantId), CancellationToken.None);

        result.Status.Should().Be("Succeeded");
        result.ProjectedTenantCount.Should().Be(1);
        result.LastSuccessfulRunAtUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        result.TenantSummarySnapshotAtUtc.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
    }

    private static async Task<AnalyticsReportingDbContext> CreateContextAsync(Guid tenantId)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<AnalyticsReportingDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AnalyticsReportingDbContext(options, new TestTenantContext(tenantId));
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private sealed class TestTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
    }
}
