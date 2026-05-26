// <copyright file="TicketSearchIntegrationEventFactoryTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.RegularExpressions;
using FluentAssertions;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Infrastructure.Services;
using NetMetric.CRM.Types;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.CRM.TicketManagement.UnitTests.Outbox;

public sealed class TicketSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateTicketIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var ticket = CreateTicket();

        var integrationEvent = TicketSearchIntegrationEventFactory.CreateTicketIndexRequested(
            ticket,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("ticket");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(TicketSearchIntegrationEventFactory.TicketReadPermission);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/tickets/{ticket.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void CreateTicketIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var ticket = CreateTicket();
        var expectedId = $"crm-ticket-{tenantId:N}-{ticket.Id:N}";

        var integrationEvent = TicketSearchIntegrationEventFactory.CreateTicketIndexRequested(ticket, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateTicketIndexRequested_Should_Contain_Only_Safe_Content_Fields()
    {
        var tenantId = Guid.NewGuid();
        var ticket = CreateTicket();
        ticket.Description = "private description";
        ticket.SetNotes("private note");
        ticket.Comments.Add(new TicketComment { Comment = "private comment" });

        var integrationEvent = TicketSearchIntegrationEventFactory.CreateTicketIndexRequested(ticket, tenantId, null, null, DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(ticket.Subject);
        integrationEvent.Document.Content.Should().Contain(ticket.TicketNumber);
        integrationEvent.Document.Content.Should().NotContain("private description");
        integrationEvent.Document.Content.Should().NotContain("private note");
        integrationEvent.Document.Content.Should().NotContain("private comment");
    }

    [Fact]
    public void CreateTicketDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var ticket = CreateTicket();

        var indexEvent = TicketSearchIntegrationEventFactory.CreateTicketIndexRequested(ticket, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);
        var deleteEvent = TicketSearchIntegrationEventFactory.CreateTicketDeleteRequested(ticket.Id, tenantId, "corr-1", "cause-1", DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("ticket");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Ticket CreateTicket()
        => new()
        {
            TicketNumber = "TKT-2026-0001",
            Subject = "Localization issue",
            Description = "Customer cannot switch language",
            TicketType = TicketType.Support,
            Channel = TicketChannelType.Web,
            Priority = PriorityType.Medium,
            Status = TicketStatusType.New,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
}
