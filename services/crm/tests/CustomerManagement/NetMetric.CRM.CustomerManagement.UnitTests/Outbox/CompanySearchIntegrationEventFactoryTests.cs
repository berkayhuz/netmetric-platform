// <copyright file="CompanySearchIntegrationEventFactoryTests.cs" company="NetMetric">
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

public sealed class CompanySearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateCompanyIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var company = CreateCompany();
        var occurredAtUtc = DateTimeOffset.UtcNow;

        var integrationEvent = CompanySearchIntegrationEventFactory.CreateCompanyIndexRequested(
            company,
            tenantId,
            "corr-1",
            "cause-1",
            occurredAtUtc);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("company");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(CompanySearchIntegrationEventFactory.CompanyReadPermission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/companies/{company.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
        integrationEvent.Document.Metadata.Should().ContainKey("entityId").WhoseValue.Should().Be(company.Id.ToString("N"));
        integrationEvent.Document.Metadata.Should().ContainKey("entityType").WhoseValue.Should().Be("company");
        integrationEvent.Document.Metadata.Should().ContainKey("tenantId").WhoseValue.Should().Be(tenantId.ToString("N"));
    }

    [Fact]
    public void CreateCompanyIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var company = CreateCompany();
        var expectedId = $"crm-company-{tenantId:N}-{company.Id:N}";

        var integrationEvent = CompanySearchIntegrationEventFactory.CreateCompanyIndexRequested(
            company,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateCompanyIndexRequested_Should_Not_Include_Sensitive_Fields_In_Content()
    {
        var tenantId = Guid.NewGuid();
        var company = CreateCompany();
        company.TaxNumber = "1234567890";
        company.TaxOffice = "Private Tax Office";
        company.Email = "private-company@example.com";
        company.Phone = "+90 212 000 00 00";
        company.SetNotes("private company notes");

        var integrationEvent = CompanySearchIntegrationEventFactory.CreateCompanyIndexRequested(
            company,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(company.Name);
        integrationEvent.Document.Content.Should().Contain(company.Website!);
        integrationEvent.Document.Content.Should().NotContain("1234567890");
        integrationEvent.Document.Content.Should().NotContain("Private Tax Office");
        integrationEvent.Document.Content.Should().NotContain("private-company@example.com");
        integrationEvent.Document.Content.Should().NotContain("+90 212 000 00 00");
        integrationEvent.Document.Content.Should().NotContain("private company notes");
    }

    [Fact]
    public void CreateCompanyDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var company = CreateCompany();
        var indexEvent = CompanySearchIntegrationEventFactory.CreateCompanyIndexRequested(
            company,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        var deleteEvent = CompanySearchIntegrationEventFactory.CreateCompanyDeleteRequested(
            company.Id,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("company");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Company CreateCompany()
        => new()
        {
            Name = "Contoso Holding",
            Website = "https://contoso.example",
            CompanyType = CompanyType.Prospect,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
}
