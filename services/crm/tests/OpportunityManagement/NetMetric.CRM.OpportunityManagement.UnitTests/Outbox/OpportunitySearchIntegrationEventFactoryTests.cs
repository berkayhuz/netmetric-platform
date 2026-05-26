// <copyright file="OpportunitySearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.OpportunityManagement.UnitTests.Outbox;

public sealed class OpportunitySearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateOpportunityIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var opportunity = CreateOpportunity();

        var integrationEvent = OpportunitySearchIntegrationEventFactory.CreateOpportunityIndexRequested(opportunity, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("opportunity");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(OpportunitySearchIntegrationEventFactory.OpportunityReadPermission);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/opportunities/{opportunity.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateOpportunityIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var opportunity = CreateOpportunity();
        var expectedId = $"crm-opportunity-{tenantId:N}-{opportunity.Id:N}";

        var integrationEvent = OpportunitySearchIntegrationEventFactory.CreateOpportunityIndexRequested(opportunity, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateOpportunityIndexRequested_Should_Exclude_Unsafe_Freeform_And_Financial_Fields()
    {
        var tenantId = Guid.NewGuid();
        var opportunity = CreateOpportunity();
        opportunity.Description = "private commercial description";
        opportunity.EstimatedAmount = 125000m;
        opportunity.ExpectedRevenue = 150000m;
        opportunity.Probability = 75m;
        opportunity.LostNote = "private lost note";
        opportunity.SetNotes("private internal note");

        var integrationEvent = OpportunitySearchIntegrationEventFactory.CreateOpportunityIndexRequested(opportunity, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(opportunity.Name);
        integrationEvent.Document.Content.Should().Contain(opportunity.OpportunityCode);
        integrationEvent.Document.Content.Should().NotContain("private commercial description");
        integrationEvent.Document.Content.Should().NotContain("125000");
        integrationEvent.Document.Content.Should().NotContain("150000");
        integrationEvent.Document.Content.Should().NotContain("75");
        integrationEvent.Document.Content.Should().NotContain("private lost note");
        integrationEvent.Document.Content.Should().NotContain("private internal note");
    }

    [Fact]
    public void CreateOpportunityDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var opportunity = CreateOpportunity();

        var indexEvent = OpportunitySearchIntegrationEventFactory.CreateOpportunityIndexRequested(opportunity, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);
        var deleteEvent = OpportunitySearchIntegrationEventFactory.CreateOpportunityDeleteRequested(opportunity.Id, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("opportunity");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Opportunity CreateOpportunity()
        => new()
        {
            OpportunityCode = "OPP-2026-0001",
            Name = "Enterprise expansion",
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
}
