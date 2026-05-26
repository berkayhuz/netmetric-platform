// <copyright file="DealSearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.DealManagement.Infrastructure.Services;
using NetMetric.CRM.Sales;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.DealManagement.UnitTests.Outbox;

public sealed class DealSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateDealIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var deal = CreateDeal();

        var integrationEvent = DealSearchIntegrationEventFactory.CreateDealIndexRequested(
            deal,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("deal");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(DealSearchIntegrationEventFactory.DealReadPermission);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/deals/{deal.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateDealIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var deal = CreateDeal();
        var expectedId = $"crm-deal-{tenantId:N}-{deal.Id:N}";

        var integrationEvent = DealSearchIntegrationEventFactory.CreateDealIndexRequested(
            deal,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateDealIndexRequested_Should_Contain_Only_Safe_Content_Fields()
    {
        var tenantId = Guid.NewGuid();
        var deal = CreateDeal();
        deal.SetNotes("private note");
        deal.LostNote = "private lost note";

        var integrationEvent = DealSearchIntegrationEventFactory.CreateDealIndexRequested(
            deal,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(deal.Name);
        integrationEvent.Document.Content.Should().Contain(deal.DealCode);
        integrationEvent.Document.Content.Should().NotContain("private note");
        integrationEvent.Document.Content.Should().NotContain("private lost note");
    }

    [Fact]
    public void CreateDealDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var deal = CreateDeal();

        var indexEvent = DealSearchIntegrationEventFactory.CreateDealIndexRequested(deal, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);
        var deleteEvent = DealSearchIntegrationEventFactory.CreateDealDeleteRequested(deal.Id, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("deal");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Deal CreateDeal()
        => new()
        {
            DealCode = "DEAL-2026-0001",
            Name = "Enterprise annual contract",
            TotalAmount = 100000m,
            ClosedDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
}
