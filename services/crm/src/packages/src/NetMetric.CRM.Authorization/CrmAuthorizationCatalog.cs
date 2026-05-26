// <copyright file="CrmAuthorizationCatalog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Authorization;

public static class CrmAuthorizationCatalog
{
    public const string WildcardPermission = "*";
    public const string CustomersResource = "customers";
    public const string ContactsResource = "contacts";
    public const string CompaniesResource = "companies";
    public const string LeadsResource = "leads";
    public const string OpportunitiesResource = "opportunities";
    public const string DealsResource = "deals";
    public const string QuotesResource = "quotes";
    public const string TicketsResource = "tickets";
    public const string DocumentsResource = "documents";
    public const string CustomersRead = "customers.read";
    public const string CustomersManage = "customers.manage";
    public const string CustomersAssignedRead = "customers.assigned.read";
    public const string CustomersSensitiveRead = "customers.sensitive.read";
    public const string CustomersInternalNotesRead = "customers.internal-notes.read";
    public const string ContactsSensitiveRead = "contacts.sensitive.read";
    public const string ContactsInternalNotesRead = "contacts.internal-notes.read";
    public const string CompaniesSensitiveRead = "companies.sensitive.read";
    public const string CompaniesFinancialRead = "companies.financial.read";
    public const string CompaniesInternalNotesRead = "companies.internal-notes.read";
    public const string LeadsSensitiveRead = "leads.sensitive.read";
    public const string LeadsFinancialRead = "leads.financial.read";
    public const string LeadsInternalNotesRead = "leads.internal-notes.read";
    public const string OpportunitiesFinancialRead = "opportunities.financial.read";
    public const string OpportunitiesInternalNotesRead = "opportunities.internal-notes.read";
    public const string DealsFinancialRead = "deals.financial.read";
    public const string DealsInternalNotesRead = "deals.internal-notes.read";
    public const string QuotesFinancialRead = "quotes.financial.read";
    public const string QuotesInternalNotesRead = "quotes.internal-notes.read";
    public const string TicketsInternalNotesRead = "tickets.internal-notes.read";
    public const string DocumentsPreviewRead = "documents.preview.read";

    public static bool HasPermission(IEnumerable<string> permissions, string permission) =>
        permissions.Contains(WildcardPermission, StringComparer.OrdinalIgnoreCase) ||
        permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
}
