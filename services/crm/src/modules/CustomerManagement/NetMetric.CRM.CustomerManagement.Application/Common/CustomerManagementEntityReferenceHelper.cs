// <copyright file="CustomerManagementEntityReferenceHelper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Activities;
using NetMetric.CRM.Documents;
using NetMetric.CRM.SharedKernel;
using NetMetric.CRM.Tagging;
using NetMetric.Exceptions;

namespace NetMetric.CRM.CustomerManagement.Application.Common;

public static class CustomerManagementEntityReferenceHelper
{
    public static void ApplyReference(Note note, string entityName, Guid entityId)
    {
        note.CompanyId = null;
        note.ContactId = null;
        note.CustomerId = null;
        note.LeadId = null;
        note.OpportunityId = null;
        note.TicketId = null;

        switch (entityName.Trim().ToLowerInvariant())
        {
            case EntityNames.Company:
                note.CompanyId = entityId;
                break;
            case EntityNames.Contact:
                note.ContactId = entityId;
                break;
            case EntityNames.Customer:
                note.CustomerId = entityId;
                break;
            default:
                throw InvalidEntityName(entityName);
        }
    }

    public static void ApplyReference(TagMap tagMap, string entityName, Guid entityId)
    {
        var normalizedEntityName = entityName.Trim().ToLowerInvariant();
        if (!EntityNames.IsSupported(normalizedEntityName))
            throw InvalidEntityName(entityName);

        tagMap.ReassignEntity(new EntityReference(normalizedEntityName, entityId));
    }

    public static void ApplyReference(Document document, string entityName, Guid entityId)
    {
        document.CompanyId = null;
        document.ContactId = null;
        document.CustomerId = null;

        switch (entityName.Trim().ToLowerInvariant())
        {
            case EntityNames.Company:
                document.CompanyId = entityId;
                break;
            case EntityNames.Contact:
                document.ContactId = entityId;
                break;
            case EntityNames.Customer:
                document.CustomerId = entityId;
                break;
            default:
                throw InvalidEntityName(entityName);
        }
    }

    private static ValidationAppException InvalidEntityName(string entityName)
    {
        ArgumentNullException.ThrowIfNull(entityName);
        return new(
            "Unsupported entity name.",
            new Dictionary<string, string[]>
            {
                [nameof(entityName)] = ["Entity name must be company, contact or customer."]
            });
    }
}
