// <copyright file="PipelineTenantIsolationTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Handlers;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.PipelineManagement.UnitTests;

public sealed class PipelineTenantIsolationTests
{
    [Fact]
    public async Task DbContext_Should_Filter_Pipeline_Data_By_Current_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();

        await using var fixture = await PipelineDbFixture.CreateAsync(tenantA);
        fixture.Db.Leads.Add(CreateLead(tenantA, "A-1"));
        fixture.Db.Leads.Add(CreateLead(tenantB, "B-1"));
        await fixture.Db.SaveChangesAsync();

        var visibleLeads = await fixture.Db.Leads.ToListAsync();

        visibleLeads.Should().ContainSingle();
        visibleLeads[0].TenantId.Should().Be(tenantA);
    }

    [Fact]
    public async Task Lead_Conversion_Preview_Should_Not_Read_Cross_Tenant_Lead()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        await using var fixture = await PipelineDbFixture.CreateAsync(tenantA);
        var foreignLead = CreateLead(tenantB, "B-1");
        fixture.Db.Leads.Add(foreignLead);
        await fixture.Db.SaveChangesAsync();

        var handler = new GetLeadConversionPreviewQueryHandler(
            fixture.Db,
            new FakeCurrentUserService(tenantA));

        var act = () => handler.Handle(new GetLeadConversionPreviewQuery(foreignLead.Id), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundAppException>();
    }

    private static Lead CreateLead(Guid tenantId, string code)
        => new()
        {
            TenantId = tenantId,
            LeadCode = code,
            FirstName = "Test",
            LastName = "Lead",
            FullName = "Test Lead",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

    private sealed class FakeTenantContext(Guid tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private sealed class FakeCurrentUserService(Guid tenantId) : ICurrentUserService
    {
        public Guid UserId { get; } = Guid.NewGuid();
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => true;
        public string? UserName => "test-user";
        public string? Email => "test@example.test";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions => [];
        public bool IsInRole(string role) => false;
        public bool HasPermission(string permission) => false;
    }

    private sealed class PipelineDbFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private PipelineDbFixture(SqliteConnection connection, PipelineManagementDbContext db)
        {
            _connection = connection;
            Db = db;
        }

        public PipelineManagementDbContext Db { get; }

        public static async Task<PipelineDbFixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<PipelineManagementDbContext>()
                .UseSqlite(connection)
                .Options;

            var db = new PipelineManagementDbContext(options, new FakeTenantContext(tenantId));
            await db.Database.EnsureCreatedAsync();
            return new PipelineDbFixture(connection, db);
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
