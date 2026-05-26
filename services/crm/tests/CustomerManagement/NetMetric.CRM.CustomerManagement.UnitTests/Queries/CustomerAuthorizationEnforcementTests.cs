// <copyright file="CustomerAuthorizationEnforcementTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Queries.Companies;
using NetMetric.CRM.CustomerManagement.Application.Queries.Contacts;
using NetMetric.CRM.CustomerManagement.Application.Queries.Customers;
using NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;
using NetMetric.CRM.Types;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Queries;

public sealed class CustomerAuthorizationEnforcementTests
{
    [Fact]
    public async Task GetCustomers_enforces_tenant_and_owner_scope_and_masks_contact_fields()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedAsync(
            Customer(tenantId, userId, "Ada", "Owner", "ada@example.test"),
            Customer(tenantId, Guid.NewGuid(), "Ben", "Other", "ben@example.test"),
            Customer(Guid.NewGuid(), userId, "Cy", "Tenant", "cy@example.test"));

        var handler = new GetCustomersQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.CustomersResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());

        var result = await handler.Handle(new GetCustomersQuery(), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].FullName.Should().Be("Ada Owner");
        result.Items[0].Email.Should().BeNull();
        result.Items[0].MobilePhone.Should().BeNull();
    }

    [Fact]
    public async Task GetCustomerById_applies_field_visibility_for_sensitive_detail_fields()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var customer = Customer(tenantId, userId, "Ada", "Owner", "ada@example.test");
        customer.IdentityNumber = "12345678900";
        customer.SetNotes("internal note");
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedAsync(customer);

        var restrictedHandler = new GetCustomerByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.CustomersResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());
        var restricted = await restrictedHandler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        restricted.Should().NotBeNull();
        restricted!.Email.Should().BeNull();
        restricted.MobilePhone.Should().BeNull();
        restricted.IdentityNumber.Should().BeNull();
        restricted.Notes.Should().BeNull();

        var privilegedHandler = new GetCustomerByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(
                tenantId,
                userId,
                CrmAuthorizationCatalog.CustomersResource,
                RowAccessLevel.Own,
                [CrmAuthorizationCatalog.CustomersSensitiveRead, CrmAuthorizationCatalog.CustomersInternalNotesRead])),
            new DefaultFieldAuthorizationService());
        var privileged = await privilegedHandler.Handle(new GetCustomerByIdQuery(customer.Id), CancellationToken.None);

        privileged.Should().NotBeNull();
        privileged!.Email.Should().Be("ada@example.test");
        privileged.MobilePhone.Should().Be("+15550100");
        privileged.IdentityNumber.Should().Be("12345678900");
        privileged.Notes.Should().Be("internal note");
    }

    [Fact]
    public async Task GetContacts_enforces_row_scope_and_masks_sensitive_contact_fields()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedContactsAsync(
            Contact(tenantId, userId, "Ada", "Owner", "ada.contact@example.test"),
            Contact(tenantId, Guid.NewGuid(), "Ben", "Other", "ben.contact@example.test"),
            Contact(Guid.NewGuid(), userId, "Cy", "Tenant", "cy.contact@example.test"));

        var handler = new GetContactsQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.ContactsResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());

        var result = await handler.Handle(new GetContactsQuery(), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].FullName.Should().Be("Ada Owner");
        result.Items[0].Email.Should().BeNull();
        result.Items[0].MobilePhone.Should().BeNull();
    }

    [Fact]
    public async Task GetContactById_masks_contact_notes_unless_permission_allows_them()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var contact = Contact(tenantId, userId, "Ada", "Owner", "ada.contact@example.test");
        contact.WorkPhone = "+15550900";
        contact.PersonalPhone = "+15550901";
        contact.SetNotes("contact internal note");
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedContactsAsync(contact);

        var restrictedHandler = new GetContactByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.ContactsResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());
        var restricted = await restrictedHandler.Handle(new GetContactByIdQuery(contact.Id), CancellationToken.None);

        restricted.Should().NotBeNull();
        restricted!.Email.Should().BeNull();
        restricted.MobilePhone.Should().BeNull();
        restricted.WorkPhone.Should().BeNull();
        restricted.PersonalPhone.Should().BeNull();
        restricted.Notes.Should().BeNull();

        var privilegedHandler = new GetContactByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(
                tenantId,
                userId,
                CrmAuthorizationCatalog.ContactsResource,
                RowAccessLevel.Own,
                [CrmAuthorizationCatalog.ContactsSensitiveRead, CrmAuthorizationCatalog.ContactsInternalNotesRead])),
            new DefaultFieldAuthorizationService());
        var privileged = await privilegedHandler.Handle(new GetContactByIdQuery(contact.Id), CancellationToken.None);

        privileged.Should().NotBeNull();
        privileged!.Email.Should().Be("ada.contact@example.test");
        privileged.MobilePhone.Should().Be("+15550200");
        privileged.WorkPhone.Should().Be("+15550900");
        privileged.PersonalPhone.Should().Be("+15550901");
        privileged.Notes.Should().Be("contact internal note");
    }

    [Fact]
    public async Task GetCompanies_enforces_row_scope_and_masks_contact_fields()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedCompaniesAsync(
            Company(tenantId, userId, "OwnerCo", "owner@example.test"),
            Company(tenantId, Guid.NewGuid(), "OtherCo", "other@example.test"),
            Company(Guid.NewGuid(), userId, "TenantCo", "tenant@example.test"));

        var handler = new GetCompaniesQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.CompaniesResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());

        var result = await handler.Handle(new GetCompaniesQuery(), CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].Name.Should().Be("OwnerCo");
        result.Items[0].Email.Should().BeNull();
        result.Items[0].Phone.Should().BeNull();
    }

    [Fact]
    public async Task GetCompanyById_masks_tax_financial_contact_and_notes_without_permissions()
    {
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var company = Company(tenantId, userId, "OwnerCo", "owner@example.test");
        company.TaxNumber = "TX-123";
        company.TaxOffice = "Istanbul";
        company.AnnualRevenue = 12345m;
        company.SetNotes("company internal note");
        await using var fixture = await CustomerQueryFixture.CreateAsync(tenantId);
        await fixture.SeedCompaniesAsync(company);

        var restrictedHandler = new GetCompanyByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(tenantId, userId, CrmAuthorizationCatalog.CompaniesResource, RowAccessLevel.Own, [])),
            new DefaultFieldAuthorizationService());
        var restricted = await restrictedHandler.Handle(new GetCompanyByIdQuery(company.Id), CancellationToken.None);

        restricted.Should().NotBeNull();
        restricted!.Email.Should().BeNull();
        restricted.Phone.Should().BeNull();
        restricted.TaxNumber.Should().BeNull();
        restricted.TaxOffice.Should().BeNull();
        restricted.AnnualRevenue.Should().BeNull();
        restricted.Notes.Should().BeNull();

        var privilegedHandler = new GetCompanyByIdQueryHandler(
            fixture.DbContext,
            new FixedAuthorizationScope(new AuthorizationScope(
                tenantId,
                userId,
                CrmAuthorizationCatalog.CompaniesResource,
                RowAccessLevel.Own,
                [
                    CrmAuthorizationCatalog.CompaniesSensitiveRead,
                    CrmAuthorizationCatalog.CompaniesFinancialRead,
                    CrmAuthorizationCatalog.CompaniesInternalNotesRead
                ])),
            new DefaultFieldAuthorizationService());
        var privileged = await privilegedHandler.Handle(new GetCompanyByIdQuery(company.Id), CancellationToken.None);

        privileged.Should().NotBeNull();
        privileged!.Email.Should().Be("owner@example.test");
        privileged.Phone.Should().Be("+15550300");
        privileged.TaxNumber.Should().Be("TX-123");
        privileged.TaxOffice.Should().Be("Istanbul");
        privileged.AnnualRevenue.Should().Be(12345m);
        privileged.Notes.Should().Be("company internal note");
    }

    private static Customer Customer(Guid tenantId, Guid ownerUserId, string firstName, string lastName, string email) =>
        new()
        {
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            MobilePhone = "+15550100",
            CustomerType = CustomerType.Individual,
            RowVersion = [1, 2, 3]
        };

    private static Contact Contact(Guid tenantId, Guid ownerUserId, string firstName, string lastName, string email) =>
        new()
        {
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            MobilePhone = "+15550200",
            RowVersion = [1, 2, 3]
        };

    private static Company Company(Guid tenantId, Guid ownerUserId, string name, string email) =>
        new()
        {
            TenantId = tenantId,
            OwnerUserId = ownerUserId,
            Name = name,
            Email = email,
            Phone = "+15550300",
            CompanyType = CompanyType.Customer,
            RowVersion = [1, 2, 3]
        };

    private sealed class FixedAuthorizationScope(AuthorizationScope scope) : ICurrentAuthorizationScope
    {
        public AuthorizationScope Resolve(string resource) => scope with { Resource = resource };
    }

    private sealed class FixedTenantProvider(Guid tenantId) : ITenantProvider
    {
        public Guid? TenantId => tenantId;
    }

    private sealed class CustomerQueryFixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;

        private CustomerQueryFixture(SqliteConnection connection, CustomerManagementDbContext dbContext)
        {
            this.connection = connection;
            DbContext = dbContext;
        }

        public CustomerManagementDbContext DbContext { get; }

        public static async Task<CustomerQueryFixture> CreateAsync(Guid tenantId)
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<CustomerManagementDbContext>()
                .UseSqlite(connection)
                .Options;
            var dbContext = new CustomerManagementDbContext(options, new FixedTenantProvider(tenantId));
            await dbContext.Database.EnsureCreatedAsync();
            return new CustomerQueryFixture(connection, dbContext);
        }

        public async Task SeedAsync(params Customer[] customers)
        {
            foreach (var customer in customers)
            {
                await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO Customers
                        (Id, TenantId, FirstName, LastName, Email, MobilePhone, OwnerUserId, CustomerType, Gender, IdentityNumber, Notes, IsVip, IsActive, CreatedAt, IsDeleted, RowVersion)
                    VALUES
                        ({customer.Id}, {customer.TenantId}, {customer.FirstName}, {customer.LastName}, {customer.Email}, {customer.MobilePhone}, {customer.OwnerUserId}, {(int)customer.CustomerType}, {(int)customer.Gender}, {customer.IdentityNumber}, {customer.Notes}, {customer.IsVip}, {customer.IsActive}, {DateTime.UtcNow}, {false}, {customer.RowVersion})
                    """);
            }
        }

        public async Task SeedContactsAsync(params Contact[] contacts)
        {
            foreach (var contact in contacts)
            {
                await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO Contacts
                        (Id, TenantId, FirstName, LastName, Email, MobilePhone, WorkPhone, PersonalPhone, OwnerUserId, Gender, Notes, IsPrimaryContact, IsActive, CreatedAt, IsDeleted, RowVersion)
                    VALUES
                        ({contact.Id}, {contact.TenantId}, {contact.FirstName}, {contact.LastName}, {contact.Email}, {contact.MobilePhone}, {contact.WorkPhone}, {contact.PersonalPhone}, {contact.OwnerUserId}, {(int)contact.Gender}, {contact.Notes}, {contact.IsPrimaryContact}, {contact.IsActive}, {DateTime.UtcNow}, {false}, {contact.RowVersion})
                    """);
            }
        }

        public async Task SeedCompaniesAsync(params Company[] companies)
        {
            foreach (var company in companies)
            {
                await DbContext.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO Companies
                        (Id, TenantId, Name, Email, Phone, TaxNumber, TaxOffice, AnnualRevenue, Notes, OwnerUserId, CompanyType, IsActive, CreatedAt, IsDeleted, RowVersion)
                    VALUES
                        ({company.Id}, {company.TenantId}, {company.Name}, {company.Email}, {company.Phone}, {company.TaxNumber}, {company.TaxOffice}, {company.AnnualRevenue}, {company.Notes}, {company.OwnerUserId}, {(int)company.CompanyType}, {company.IsActive}, {DateTime.UtcNow}, {false}, {company.RowVersion})
                    """);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await connection.DisposeAsync();
        }
    }
}
