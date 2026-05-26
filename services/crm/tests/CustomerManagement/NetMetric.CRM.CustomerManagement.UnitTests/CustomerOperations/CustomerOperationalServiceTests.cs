// <copyright file="CustomerOperationalServiceTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Security;
using NetMetric.CRM.CustomerManagement.Application.Features.Customer360;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services.CustomerOperations;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.UnitTests.CustomerOperations;

public sealed class CustomerOperationalServiceTests
{
    [Fact]
    public async Task Duplicate_detection_finds_exact_email_and_normalized_phone_inside_tenant()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var owner = Guid.NewGuid();
        var source = Customer(tenantId, owner, "Ada", "Lovelace", "ADA@EXAMPLE.TEST", "+1 (555) 0100");
        var match = Customer(tenantId, owner, "Ada", "Lovelace", "ada@example.test", "15550100");
        var otherTenant = Customer(Guid.NewGuid(), owner, "Ada", "Lovelace", "ada@example.test", "15550100");
        await fixture.SeedCustomersAsync(source, match, otherTenant);

        var service = new DuplicateDetectionService(fixture.DbContext);
        var result = await service.FindCustomerDuplicatesAsync(source, CancellationToken.None);

        result.Should().ContainSingle();
        result[0].CandidateId.Should().Be(match.Id);
        result[0].Reasons.Should().Contain("Exact email match");
        result[0].Reasons.Should().Contain("Normalized phone match");
    }

    [Fact]
    public void Data_quality_penalizes_missing_invalid_and_duplicate_risk()
    {
        var customer = Customer(Guid.NewGuid(), Guid.Empty, "", "", "not-an-email", "12");
        var service = new CustomerDataQualityService();

        var snapshot = service.Calculate(customer, [], 80, DateTime.UtcNow.AddDays(-200));

        snapshot.Score.Should().BeLessThan(50);
        snapshot.MissingFieldsJson.Should().Contain("FirstName");
        snapshot.InvalidFieldsJson.Should().Contain("Email");
        snapshot.RecommendationsJson.Should().Contain("Review duplicate warning");
    }

    [Fact]
    public void Relationship_health_drops_for_stale_activity_overdue_tickets_and_churn_risk()
    {
        var customer = Customer(Guid.NewGuid(), Guid.NewGuid(), "Ada", "Lovelace", "ada@example.test", "+15550100");
        var service = new CustomerRelationshipHealthService();

        var snapshot = service.Calculate(customer, DateTime.UtcNow.AddDays(-120), openTickets: 2, overdueTickets: 2, openOpportunities: 0, wonDeals: 0, unpaidInvoices: 1, renewalDueInDays: 20, CustomerLifecycleStage.ChurnRisk);

        snapshot.Score.Should().BeLessThan(50);
        snapshot.RiskLevel.Should().BeOneOf(CustomerRelationshipRiskLevel.High, CustomerRelationshipRiskLevel.Critical);
    }

    [Fact]
    public void Portal_links_are_empty_when_base_url_missing_and_config_based_when_present()
    {
        CustomerPortalLinkBuilder.Build(new CustomerPortalOptions(), Guid.NewGuid())
            .ProfileUrl.Should().BeNull();

        var id = Guid.NewGuid();
        var links = CustomerPortalLinkBuilder.Build(new CustomerPortalOptions { BaseUrl = "https://portal.example.test/" }, id);

        links.ProfileUrl.Should().Be($"https://portal.example.test/customers/{id}");
        links.TicketsUrl.Should().Contain(id.ToString());
    }

    [Fact]
    public async Task Import_batch_validate_marks_invalid_and_duplicate_rows_inside_tenant()
    {
        var tenantId = Guid.NewGuid();
        await using var fixture = await Fixture.CreateAsync(tenantId);
        var existing = Customer(tenantId, fixture.CurrentUser.UserId, "Ada", "Lovelace", "ada@example.test", "+1 555 0100");
        var otherTenant = Customer(Guid.NewGuid(), fixture.CurrentUser.UserId, "Ada", "Other", "ada@example.test", "+1 555 0100");
        await fixture.SeedCustomersAsync(existing, otherTenant);

        var batchId = await fixture.SeedImportBatchAsync(
            new Dictionary<string, string?> { ["customer name"] = "Ada Lovelace", ["email"] = "ada@example.test" },
            new Dictionary<string, string?> { ["customer name"] = "Grace Hopper", ["email"] = "grace@example.test" },
            new Dictionary<string, string?> { ["customer name"] = "Broken Row", ["email"] = "invalid-email" });

        var validate = new ValidateCustomerImportBatchCommandHandler(fixture.DbContext, fixture.CurrentUser, new DuplicateDetectionService(fixture.DbContext));
        var result = await validate.Handle(new ValidateCustomerImportBatchCommand(batchId), CancellationToken.None);

        result.TotalRows.Should().Be(3);
        result.DuplicateRows.Should().Be(1);
        result.InvalidRows.Should().Be(1);
        result.Rows.Should().Contain(x => x.Status == CustomerImportRowStatus.Duplicate && x.DuplicateWarningsJson!.Contains(existing.Id.ToString()));
        result.Rows.Should().Contain(x => x.Status == CustomerImportRowStatus.Invalid && x.ValidationErrorsJson!.Contains("Email is invalid"));
    }

    private static Customer Customer(Guid tenantId, Guid ownerUserId, string firstName, string lastName, string email, string phone)
        => new()
        {
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            MobilePhone = phone,
            CustomerType = CustomerType.Individual,
            CreatedAt = DateTime.UtcNow,
            RowVersion = [1, 2, 3]
        };

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private Fixture(SqliteConnection connection, CustomerManagementDbContext dbContext, Guid tenantId)
        {
            this.connection = connection;
            DbContext = dbContext;
            CurrentUser = new FixedCurrentUser(Guid.NewGuid(), tenantId);
            Security = new CustomerManagementSecurityService(CurrentUser, dbContext, Options.Create(new CustomerManagementSecurityOptions()));
        }

        public CustomerManagementDbContext DbContext { get; }
        public ICurrentUserService CurrentUser { get; }
        public ICustomerManagementSecurityService Security { get; }

        public async Task SeedCustomersAsync(params Customer[] customers)
        {
            foreach (var customer in customers)
            {
                await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO Customers
                        (Id, TenantId, FirstName, LastName, Email, MobilePhone, OwnerUserId, CustomerType, Gender, IsVip, IsActive, CreatedAt, IsDeleted, RowVersion)
                    VALUES
                        ({customer.Id}, {customer.TenantId}, {customer.FirstName}, {customer.LastName}, {customer.Email}, {customer.MobilePhone}, {customer.OwnerUserId}, {(int)customer.CustomerType}, {(int)customer.Gender}, {customer.IsVip}, {customer.IsActive}, {customer.CreatedAt}, {false}, {customer.RowVersion})
                    """);
            }
        }

        public async Task<Guid> SeedImportBatchAsync(params IReadOnlyDictionary<string, string?>[] rows)
        {
            var batchId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;
            await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                INSERT INTO CustomerImportBatches
                    (Id, TenantId, FileName, Source, Status, TotalRows, ValidRows, InvalidRows, DuplicateRows, CreatedAt, IsDeleted, IsActive, RowVersion)
                VALUES
                    ({batchId}, {CurrentUser.TenantId}, {"customers.json"}, {"UnitTest"}, {(int)CustomerImportBatchStatus.Uploaded}, {rows.Length}, {0}, {0}, {0}, {createdAt}, {false}, {true}, {new byte[] { 1, 2, 3 }})
                """);

            for (var i = 0; i < rows.Length; i++)
            {
                await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO CustomerImportRows
                        (Id, TenantId, BatchId, RowNumber, RawDataJson, Status, CreatedAt, IsDeleted, IsActive, RowVersion)
                    VALUES
                        ({Guid.NewGuid()}, {CurrentUser.TenantId}, {batchId}, {i + 1}, {System.Text.Json.JsonSerializer.Serialize(rows[i])}, {(int)CustomerImportRowStatus.Pending}, {createdAt}, {false}, {true}, {new byte[] { 1, 2, 3 }})
                    """);
            }

            return batchId;
        }

        public static async Task<Fixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>().UseSqlite(connection).Options;
            var dbContext = new CustomerManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new Fixture(connection, dbContext, tenantId);
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

    private sealed class FixedCurrentUser(Guid userId, Guid tenantId) : ICurrentUserService
    {
        public Guid UserId { get; } = userId;
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
