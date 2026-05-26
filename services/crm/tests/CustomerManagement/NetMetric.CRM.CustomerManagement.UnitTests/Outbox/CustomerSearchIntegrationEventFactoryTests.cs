// <copyright file="CustomerSearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.CustomerManagement.UnitTests.Outbox;

public sealed class CustomerSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateCustomerIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var customer = CreateCustomer();
        var occurredAtUtc = DateTimeOffset.UtcNow;

        var integrationEvent = CustomerSearchIntegrationEventFactory.CreateCustomerIndexRequested(
            customer,
            tenantId,
            "corr-1",
            "cause-1",
            occurredAtUtc);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("customer");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(CustomerSearchIntegrationEventFactory.CustomerReadPermission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/customers/{customer.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateCustomerIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var customer = CreateCustomer();
        var expectedId = $"crm-customer-{tenantId:N}-{customer.Id:N}";

        var integrationEvent = CustomerSearchIntegrationEventFactory.CreateCustomerIndexRequested(
            customer,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateCustomerIndexRequested_Should_Not_Include_Sensitive_Fields_In_Content()
    {
        var tenantId = Guid.NewGuid();
        var customer = CreateCustomer();
        customer.Email = "private@example.com";
        customer.MobilePhone = "+90 555 000 00 00";
        customer.IdentityNumber = "12345678901";
        customer.SetNotes("private notes");

        var integrationEvent = CustomerSearchIntegrationEventFactory.CreateCustomerIndexRequested(
            customer,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().NotContain("private@example.com");
        integrationEvent.Document.Content.Should().NotContain("+90 555 000 00 00");
        integrationEvent.Document.Content.Should().NotContain("12345678901");
        integrationEvent.Document.Content.Should().NotContain("private notes");
    }

    [Fact]
    public void CreateCustomerDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var customer = CreateCustomer();
        var indexEvent = CustomerSearchIntegrationEventFactory.CreateCustomerIndexRequested(
            customer,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        var deleteEvent = CustomerSearchIntegrationEventFactory.CreateCustomerDeleteRequested(
            customer.Id,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("customer");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Customer CreateCustomer()
    {
        return new Customer
        {
            FirstName = "Jane",
            LastName = "Doe",
            CustomerType = CustomerType.Corporate,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }
}
