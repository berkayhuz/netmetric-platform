// <copyright file="LeadSearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.LeadManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.Search.Application.Security;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.LeadManagement.UnitTests.Outbox;

public sealed class LeadSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateLeadIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var lead = CreateLead(tenantId);

        var integrationEvent = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(
            lead,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("lead");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(LeadSearchIntegrationEventFactory.LeadReadPermission);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/leads/{lead.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateLeadIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var lead = CreateLead(tenantId);
        var expectedId = $"crm-lead-{tenantId:N}-{lead.Id:N}";

        var integrationEvent = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(lead, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateLeadIndexRequested_Should_Contain_Only_Safe_Content_Fields()
    {
        var tenantId = Guid.NewGuid();
        var lead = CreateLead(tenantId);

        var integrationEvent = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(lead, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(lead.FullName);
        integrationEvent.Document.Content.Should().Contain(lead.LeadCode);
        integrationEvent.Document.Content.Should().Contain(lead.CompanyName);
        integrationEvent.Document.Content.Should().NotContain(lead.Email);
        integrationEvent.Document.Content.Should().NotContain(lead.Phone);
        integrationEvent.Document.Content.Should().NotContain(lead.MobilePhone);
        integrationEvent.Document.Content.Should().NotContain(lead.Description);
        integrationEvent.Document.Content.Should().NotContain(lead.Notes);
        integrationEvent.Document.Content.Should().NotContain(lead.UtmSource);
        integrationEvent.Document.Content.Should().NotContain(lead.ReferrerUrl);
    }

    [Fact]
    public void CreateLeadDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var lead = CreateLead(tenantId);

        var indexEvent = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(lead, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);
        var deleteEvent = LeadSearchIntegrationEventFactory.CreateLeadDeleteRequested(lead.Id, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("lead");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    [Fact]
    public void CreateLeadIndexRequested_Should_Pass_SearchDocumentSecurityValidator()
    {
        var tenantId = Guid.NewGuid();
        var lead = CreateLead(tenantId);
        var integrationEvent = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(lead, tenantId, null, null, DateTimeOffset.UtcNow);

        var errors = SearchDocumentSecurityValidator.Validate(integrationEvent.Document);
        errors.Should().BeEmpty();
    }

    private static Lead CreateLead(Guid tenantId)
        => new()
        {
            TenantId = tenantId,
            LeadCode = "LEAD-2026-0001",
            FullName = "Ada Lovelace",
            CompanyName = "Analytical Engines Ltd",
            Email = "ada@example.test",
            Phone = "555-11-22",
            MobilePhone = "555-22-33",
            Description = "private qualification context",
            Notes = "private notes",
            UtmSource = "newsletter",
            ReferrerUrl = "https://private.example.test",
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
}
