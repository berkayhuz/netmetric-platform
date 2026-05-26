// <copyright file="ContactSearchIntegrationEventFactoryTests.cs" company="NetMetric">
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

public sealed class ContactSearchIntegrationEventFactoryTests
{
    [Fact]
    public void CreateContactIndexRequested_Should_Map_Required_Search_Document_Fields()
    {
        var tenantId = Guid.NewGuid();
        var contact = CreateContact();
        var occurredAtUtc = DateTimeOffset.UtcNow;

        var integrationEvent = ContactSearchIntegrationEventFactory.CreateContactIndexRequested(
            contact,
            tenantId,
            "corr-1",
            "cause-1",
            occurredAtUtc);

        integrationEvent.Document.Source.Should().Be(SearchDocumentSource.Crm);
        integrationEvent.Document.Type.Should().Be("contact");
        integrationEvent.Document.Visibility.Should().Be(SearchDocumentVisibility.Permission);
        integrationEvent.Document.TenantId.Should().Be(tenantId);
        integrationEvent.Document.RequiredPermissions.Should().ContainSingle(ContactSearchIntegrationEventFactory.ContactReadPermission);
        integrationEvent.Document.PermissionMatchMode.Should().Be(SearchPermissionMatchMode.Any);
        integrationEvent.Document.Locale.Should().Be(SearchDocumentLocales.Neutral);
        integrationEvent.Document.Url.Should().Be($"/contacts/{contact.Id:D}");
        integrationEvent.Document.IsDeleted.Should().BeFalse();
        integrationEvent.Document.Metadata.Should().ContainKey("entityId").WhoseValue.Should().Be(contact.Id.ToString("N"));
        integrationEvent.Document.Metadata.Should().ContainKey("entityType").WhoseValue.Should().Be("contact");
        integrationEvent.Document.Metadata.Should().ContainKey("tenantId").WhoseValue.Should().Be(tenantId.ToString("N"));
        integrationEvent.Document.Metadata.Should().ContainKey("companyId").WhoseValue.Should().Be(contact.CompanyId!.Value.ToString("N"));
    }

    [Fact]
    public void CreateContactIndexRequested_Should_Create_Stable_Meilisearch_Safe_Id()
    {
        var tenantId = Guid.NewGuid();
        var contact = CreateContact();
        var expectedId = $"crm-contact-{tenantId:N}-{contact.Id:N}";

        var integrationEvent = ContactSearchIntegrationEventFactory.CreateContactIndexRequested(
            contact,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Id.Should().Be(expectedId);
        Regex.IsMatch(integrationEvent.Document.Id, "^[a-z0-9-]+$").Should().BeTrue();
    }

    [Fact]
    public void CreateContactIndexRequested_Should_Not_Include_Sensitive_Fields_In_Content()
    {
        var tenantId = Guid.NewGuid();
        var contact = CreateContact();
        contact.Email = "private-contact@example.com";
        contact.MobilePhone = "+90 555 000 00 00";
        contact.WorkPhone = "+90 212 000 00 00";
        contact.PersonalPhone = "+90 216 000 00 00";
        contact.Description = "private contact description";
        contact.SetNotes("private contact notes");

        var integrationEvent = ContactSearchIntegrationEventFactory.CreateContactIndexRequested(
            contact,
            tenantId,
            null,
            null,
            DateTimeOffset.UtcNow);

        integrationEvent.Document.Content.Should().Contain(contact.FullName);
        integrationEvent.Document.Content.Should().Contain(contact.JobTitle!);
        integrationEvent.Document.Content.Should().Contain(contact.Company!.Name);
        integrationEvent.Document.Content.Should().NotContain("private-contact@example.com");
        integrationEvent.Document.Content.Should().NotContain("+90 555 000 00 00");
        integrationEvent.Document.Content.Should().NotContain("+90 212 000 00 00");
        integrationEvent.Document.Content.Should().NotContain("+90 216 000 00 00");
        integrationEvent.Document.Content.Should().NotContain("private contact description");
        integrationEvent.Document.Content.Should().NotContain("private contact notes");
    }

    [Fact]
    public void CreateContactDeleteRequested_Should_Use_Same_Document_Id_As_Index()
    {
        var tenantId = Guid.NewGuid();
        var contact = CreateContact();
        var indexEvent = ContactSearchIntegrationEventFactory.CreateContactIndexRequested(
            contact,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        var deleteEvent = ContactSearchIntegrationEventFactory.CreateContactDeleteRequested(
            contact.Id,
            tenantId,
            "corr-1",
            "cause-1",
            DateTimeOffset.UtcNow);

        deleteEvent.DocumentId.Should().Be(indexEvent.Document.Id);
        deleteEvent.Source.Should().Be(SearchDocumentSource.Crm);
        deleteEvent.Type.Should().Be("contact");
        deleteEvent.TenantId.Should().Be(tenantId);
    }

    private static Contact CreateContact()
    {
        var companyId = Guid.NewGuid();
        return new Contact
        {
            FirstName = "Ada",
            LastName = "Lovelace",
            Title = "Dr.",
            JobTitle = "Chief Architect",
            CompanyId = companyId,
            Company = new Company { Name = "Contoso Holding" },
            Gender = GenderType.Unknown,
            CreatedAt = DateTime.UtcNow.AddDays(-5),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }
}
