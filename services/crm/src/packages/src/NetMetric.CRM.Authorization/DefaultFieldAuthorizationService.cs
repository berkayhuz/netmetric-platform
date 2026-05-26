// <copyright file="DefaultFieldAuthorizationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Authorization;

public sealed class DefaultFieldAuthorizationService : IFieldAuthorizationService
{
    private static readonly IReadOnlyDictionary<string, string> SensitiveFieldPermissions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["customers.financialData"] = CrmAuthorizationCatalog.CustomersSensitiveRead,
            ["customers.internalNotes"] = CrmAuthorizationCatalog.CustomersInternalNotesRead,
            ["customers.identityNumber"] = CrmAuthorizationCatalog.CustomersSensitiveRead,
            ["customers.notes"] = CrmAuthorizationCatalog.CustomersInternalNotesRead,
            ["customers.contactData"] = CrmAuthorizationCatalog.CustomersSensitiveRead,
            ["customers.auditMarkers"] = CrmAuthorizationCatalog.CustomersManage,
            ["contacts.contactData"] = CrmAuthorizationCatalog.ContactsSensitiveRead,
            ["contacts.notes"] = CrmAuthorizationCatalog.ContactsInternalNotesRead,
            ["contacts.auditMarkers"] = "contacts.manage",
            ["companies.contactData"] = CrmAuthorizationCatalog.CompaniesSensitiveRead,
            ["companies.taxData"] = CrmAuthorizationCatalog.CompaniesSensitiveRead,
            ["companies.financialData"] = CrmAuthorizationCatalog.CompaniesFinancialRead,
            ["companies.notes"] = CrmAuthorizationCatalog.CompaniesInternalNotesRead,
            ["companies.auditMarkers"] = "companies.manage",
            ["leads.contactData"] = CrmAuthorizationCatalog.LeadsSensitiveRead,
            ["leads.financialData"] = CrmAuthorizationCatalog.LeadsFinancialRead,
            ["leads.notes"] = CrmAuthorizationCatalog.LeadsInternalNotesRead,
            ["opportunities.financialData"] = CrmAuthorizationCatalog.OpportunitiesFinancialRead,
            ["opportunities.notes"] = CrmAuthorizationCatalog.OpportunitiesInternalNotesRead,
            ["deals.financialData"] = CrmAuthorizationCatalog.DealsFinancialRead,
            ["deals.notes"] = CrmAuthorizationCatalog.DealsInternalNotesRead,
            ["quotes.financialData"] = CrmAuthorizationCatalog.QuotesFinancialRead,
            ["quotes.notes"] = CrmAuthorizationCatalog.QuotesInternalNotesRead,
            ["tickets.notes"] = CrmAuthorizationCatalog.TicketsInternalNotesRead,
            ["documents.preview"] = CrmAuthorizationCatalog.DocumentsPreviewRead
        };

    public FieldAuthorizationDecision Decide(string resource, string field, IReadOnlyCollection<string> permissions)
    {
        var key = $"{resource}.{field}";
        if (!SensitiveFieldPermissions.TryGetValue(key, out var requiredPermission))
        {
            return new FieldAuthorizationDecision(resource, field, FieldVisibility.Visible);
        }

        var visibility = CrmAuthorizationCatalog.HasPermission(permissions, requiredPermission) ||
                         CrmAuthorizationCatalog.HasPermission(permissions, $"{resource}.manage")
            ? FieldVisibility.Visible
            : FieldVisibility.Masked;

        return new FieldAuthorizationDecision(resource, field, visibility);
    }
}
