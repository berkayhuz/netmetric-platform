// <copyright file="StaticSearchDocumentRegistry.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.StaticDocuments;

public sealed class StaticSearchDocumentRegistry : IStaticSearchDocumentRegistry
{
    public StaticSearchDocumentRegistry()
        : this(new StaticSearchDocumentFactory(SearchStaticTextLocalizer.CreateDefault()))
    {
    }

    public StaticSearchDocumentRegistry(StaticSearchDocumentFactory factory)
    {
        Factory = factory;
    }

    public static IReadOnlyCollection<StaticSearchManifestItem> ManifestItems { get; } =
    [
        // Public website pages
        CreateItem("public-page-home", SearchDocumentSource.Public, "page", "public.search.home.title", "public.search.home.summary", "/", SearchDocumentVisibility.Public, tags: ["public", "home", "platform"], boost: 2.0, keywordKeys: ["public.search.home.keywords"]),
        CreateItem("public-page-pricing", SearchDocumentSource.Public, "page", "public.search.pricing.title", "public.search.pricing.summary", "/pricing", SearchDocumentVisibility.Public, tags: ["public", "pricing", "plans"], boost: 1.8, keywordKeys: ["public.search.pricing.keywords"]),
        CreateItem("public-page-product", SearchDocumentSource.Public, "page", "public.search.product.title", "public.search.product.summary", "/product", SearchDocumentVisibility.Public, tags: ["public", "product", "features"], boost: 1.6, keywordKeys: ["public.search.product.keywords"]),
        CreateItem("public-page-tools-landing", SearchDocumentSource.Public, "page", "public.search.tools.title", "public.search.tools.summary", "/tools", SearchDocumentVisibility.Public, tags: ["public", "tools", "utilities"], boost: 1.7, keywordKeys: ["public.search.tools.keywords"]),

        // Public tools
        CreateItem("tools-tool-qr-generator", SearchDocumentSource.Tools, "tool", "tools.search.qrGenerator.title", "tools.search.qrGenerator.summary", "/qr-generator", SearchDocumentVisibility.Public, tags: ["tools", "qr", "generator"], boost: 1.5, keywordKeys: ["tools.search.qrGenerator.keywords"]),
        CreateItem("tools-tool-png-to-jpg", SearchDocumentSource.Tools, "tool", "tools.search.pngToJpg.title", "tools.search.pngToJpg.summary", "/image/png-to-jpg", SearchDocumentVisibility.Public, tags: ["tools", "image", "conversion"], boost: 1.4, keywordKeys: ["tools.search.pngToJpg.keywords"]),

        // Account pages (authenticated only)
        CreateItem("account-page-profile", SearchDocumentSource.Account, "page", "account.profile.title", "account.profile.searchSummary", "/profile", SearchDocumentVisibility.Authenticated, tags: ["account", "profile", "settings"], boost: 1.5, keywordKeys: ["account.profile.searchKeywords"]),
        CreateItem("account-page-security", SearchDocumentSource.Account, "page", "account.security.title", "account.security.searchSummary", "/security", SearchDocumentVisibility.Authenticated, tags: ["account", "security", "mfa"], boost: 1.6, keywordKeys: ["account.security.searchKeywords"]),
        CreateItem("account-page-preferences", SearchDocumentSource.Account, "page", "account.preferences.title", "account.preferences.searchSummary", "/preferences", SearchDocumentVisibility.Authenticated, tags: ["account", "preferences", "settings"], boost: 1.4, keywordKeys: ["account.preferences.searchKeywords"]),
        CreateItem("account-page-workspaces", SearchDocumentSource.Account, "page", "account.workspaces.title", "account.workspaces.searchSummary", "/workspaces", SearchDocumentVisibility.Authenticated, tags: ["account", "workspaces", "organizations"], boost: 1.3, keywordKeys: ["account.workspaces.searchKeywords"]),
        CreateItem("account-page-sessions", SearchDocumentSource.Account, "page", "account.sessions.title", "account.sessions.searchSummary", "/security/sessions", SearchDocumentVisibility.Authenticated, tags: ["account", "sessions", "security"], boost: 1.5, keywordKeys: ["account.sessions.searchKeywords"]),
        CreateItem("account-page-mfa", SearchDocumentSource.Account, "page", "account.mfa.title", "account.mfa.searchSummary", "/security/mfa", SearchDocumentVisibility.Authenticated, tags: ["account", "mfa", "security"], boost: 1.5, keywordKeys: ["account.mfa.searchKeywords"]),
        CreateItem("account-page-password", SearchDocumentSource.Account, "page", "account.security.password.title", "account.security.password.searchSummary", "/security/password", SearchDocumentVisibility.Authenticated, tags: ["account", "password", "security"], boost: 1.4, keywordKeys: ["account.security.password.searchKeywords"]),
        CreateItem("account-page-notifications", SearchDocumentSource.Account, "page", "account.notifications.title", "account.notifications.searchSummary", "/notifications", SearchDocumentVisibility.Authenticated, tags: ["account", "notifications", "preferences"], boost: 1.2, keywordKeys: ["account.notifications.searchKeywords"]),
        CreateItem("account-page-audit", SearchDocumentSource.Account, "page", "account.audit.title", "account.audit.searchSummary", "/audit", SearchDocumentVisibility.Authenticated, tags: ["account", "audit", "activity"], boost: 1.2, keywordKeys: ["account.audit.searchKeywords"]),
        CreateItem("account-page-team", SearchDocumentSource.Account, "page", "account.team.title", "account.team.searchSummary", "/settings/team", SearchDocumentVisibility.Authenticated, tags: ["account", "team", "settings"], boost: 1.2, keywordKeys: ["account.team.searchKeywords"]),
        CreateItem("account-page-privacy", SearchDocumentSource.Account, "page", "account.privacy.title", "account.privacy.searchSummary", "/privacy", SearchDocumentVisibility.Authenticated, tags: ["account", "privacy", "consent"], boost: 1.2, keywordKeys: ["account.privacy.searchKeywords"]),

        // CRM module navigation (permission-gated)
        CreateCrmModule("crm-module-customers", "crm.modules.customers", "crm.modules.customers.summary", "/customers", ["crm.customer-management.customers.read"], ["crm", "customers", "navigation"], 1.8, ["crm.modules.customers.keywords"]),
        CreateCrmModule("crm-module-companies", "crm.modules.companies", "crm.modules.companies.summary", "/companies", ["crm.customer-management.companies.read"], ["crm", "companies", "navigation"], 1.7, ["crm.modules.companies.keywords"]),
        CreateCrmModule("crm-module-contacts", "crm.modules.contacts", "crm.modules.contacts.summary", "/contacts", ["crm.customer-management.contacts.read"], ["crm", "contacts", "navigation"], 1.7, ["crm.modules.contacts.keywords"]),
        CreateCrmModule("crm-module-customer-intelligence", "crm.modules.customer-intelligence.title", "crm.modules.customer-intelligence.summary", "/customer-intelligence", ["customer-intelligence.search.read"], ["crm", "customer-intelligence", "insights"], 1.6, ["crm.modules.customer-intelligence.keywords"]),
        CreateCrmModule("crm-module-leads", "crm.modules.lead-management.title", "crm.modules.lead-management.summary", "/leads", ["leads.read"], ["crm", "leads"], 1.5, ["crm.modules.lead-management.keywords"]),
        CreateCrmModule("crm-module-deals", "crm.modules.deal-management.title", "crm.modules.deal-management.summary", "/deals", ["deals.read"], ["crm", "deals"], 1.5, ["crm.modules.deal-management.keywords"]),
        CreateCrmModule("crm-module-opportunities", "crm.modules.opportunity-management.title", "crm.modules.opportunity-management.summary", "/opportunities", ["opportunities.read"], ["crm", "opportunities"], 1.5, ["crm.modules.opportunity-management.keywords"]),
        CreateCrmModule("crm-module-pipeline", "crm.modules.pipeline-management.title", "crm.modules.pipeline-management.summary", "/pipeline", ["pipeline.pipelines.read"], ["crm", "pipeline"], 1.5, ["crm.modules.pipeline-management.keywords"]),
        CreateCrmModule("crm-module-quotes", "crm.modules.quote-management.title", "crm.modules.quote-management.summary", "/quotes", ["quotes.read"], ["crm", "quotes"], 1.5, ["crm.modules.quote-management.keywords"]),
        CreateCrmModule("crm-module-sales-forecasting", "crm.modules.sales-forecasting.title", "crm.modules.sales-forecasting.summary", "/sales-forecasting", ["sales-forecasts.read"], ["crm", "sales-forecasting"], 1.4, ["crm.modules.sales-forecasting.keywords"]),
        CreateCrmModule("crm-module-product-catalog", "crm.modules.product-catalog.title", "crm.modules.product-catalog.summary", "/product-catalog", ["catalog.products.read"], ["crm", "product-catalog"], 1.4, ["crm.modules.product-catalog.keywords"]),
        CreateCrmModule("crm-module-support-inbox", "crm.modules.support-inbox.title", "crm.modules.support-inbox.summary", "/support-inbox", ["crm.inbox.read"], ["crm", "support-inbox"], 1.4, ["crm.modules.support-inbox.keywords"]),
        CreateCrmModule("crm-module-tickets", "crm.modules.ticket-management.title", "crm.modules.ticket-management.summary", "/tickets", ["tickets.read"], ["crm", "tickets"], 1.5, ["crm.modules.ticket-management.keywords"]),
        CreateCrmModule("crm-module-ticket-sla", "crm.modules.ticket-sla.title", "crm.modules.ticket-sla.summary", "/ticket-sla", ["ticket.sla-policies.read"], ["crm", "ticket-sla"], 1.3, ["crm.modules.ticket-sla.keywords"]),
        CreateCrmModule("crm-module-ticket-workflows", "crm.modules.ticket-workflow.title", "crm.modules.ticket-workflow.summary", "/ticket-workflows", ["ticket.queues.read"], ["crm", "ticket-workflows"], 1.3, ["crm.modules.ticket-workflow.keywords"]),
        CreateCrmModule("crm-module-marketing", "crm.modules.marketing-automation.title", "crm.modules.marketing-automation.summary", "/marketing", ["marketing.campaigns.read"], ["crm", "marketing", "automation"], 1.3, ["crm.modules.marketing-automation.keywords"]),
        CreateCrmModule("crm-module-omnichannel", "crm.modules.omnichannel.title", "crm.modules.omnichannel.summary", "/omnichannel", ["omnichannel.read"], ["crm", "omnichannel"], 1.3, ["crm.modules.omnichannel.keywords"]),
        CreateCrmModule("crm-module-calendar-sync", "crm.modules.calendar-sync.title", "crm.modules.calendar-sync.summary", "/calendar-sync", ["calendar-sync.read"], ["crm", "calendar-sync"], 1.2, ["crm.modules.calendar-sync.keywords"]),
        CreateCrmModule("crm-module-contracts", "crm.modules.contract-lifecycle.title", "crm.modules.contract-lifecycle.summary", "/contracts", ["contracts.read"], ["crm", "contracts"], 1.2, ["crm.modules.contract-lifecycle.keywords"]),
        CreateCrmModule("crm-module-documents", "crm.modules.document-management.title", "crm.modules.document-management.summary", "/documents", ["documents.read"], ["crm", "documents"], 1.2, ["crm.modules.document-management.keywords"]),
        CreateCrmModule("crm-module-finance", "crm.modules.finance-operations.title", "crm.modules.finance-operations.summary", "/finance", ["finance.operations.read"], ["crm", "finance"], 1.2, ["crm.modules.finance-operations.keywords"]),
        CreateCrmModule("crm-module-integrations", "crm.modules.integration-hub.title", "crm.modules.integration-hub.summary", "/integrations", ["integrations.read"], ["crm", "integrations"], 1.2, ["crm.modules.integration-hub.keywords"]),
        CreateCrmModule("crm-module-knowledge-base", "crm.modules.knowledge-base.title", "crm.modules.knowledge-base.summary", "/knowledge-base", ["knowledge-base.articles.read"], ["crm", "knowledge-base"], 1.2, ["crm.modules.knowledge-base.keywords"]),
        CreateCrmModule("crm-module-work-management", "crm.modules.work-management.title", "crm.modules.work-management.summary", "/work-management", ["work-management.tasks.read"], ["crm", "work-management"], 1.2, ["crm.modules.work-management.keywords"]),
        CreateCrmModule("crm-module-workflows", "crm.modules.workflow-automation.title", "crm.modules.workflow-automation.summary", "/workflows", ["workflow.rules.manage"], ["crm", "workflows"], 1.2, ["crm.modules.workflow-automation.keywords"]),
        CreateCrmModule("crm-module-tasks", "crm.modules.tasks.title", "crm.modules.tasks.summary", "/tasks", ["tasks.read"], ["crm", "tasks"], 1.2, ["crm.modules.tasks.keywords"]),
        CreateCrmModule("crm-module-activities", "crm.modules.activities.title", "crm.modules.activities.summary", "/activities", ["activities.read"], ["crm", "activities"], 1.2, ["crm.modules.activities.keywords"]),
        CreateCrmModule("crm-module-analytics", "crm.modules.analytics-reporting.title", "crm.modules.analytics-reporting.summary", "/analytics", ["analytics.read"], ["crm", "analytics"], 1.2, ["crm.modules.analytics-reporting.keywords"]),
        CreateCrmModule("crm-module-ai", "crm.modules.artificial-intelligence.title", "crm.modules.artificial-intelligence.summary", "/ai", ["artificial-intelligence.read"], ["crm", "ai"], 1.2, ["crm.modules.artificial-intelligence.keywords"]),
        CreateCrmModule("crm-module-tags", "crm.modules.tag-management.title", "crm.modules.tag-management.summary", "/tags", ["tags.read"], ["crm", "tags"], 1.2, ["crm.modules.tag-management.keywords"]),
        CreateCrmModule("crm-module-tenants", "crm.modules.tenant-management.title", "crm.modules.tenant-management.summary", "/tenants", ["tenants.read"], ["crm", "tenants"], 1.2, ["crm.modules.tenant-management.keywords"]),
        CreateCrmModule("crm-module-settings", "crm.modules.settings.title", "crm.modules.settings.summary", "/settings", ["crm.settings.read"], ["crm", "settings", "navigation"], 1.6, ["crm.modules.settings.keywords"]),

        // CRM common subpages
        CreateCrmPage("crm-page-customers-list", "crm.search.pages.customersList.title", "crm.search.pages.customersList.summary", "/customers", ["crm.customer-management.customers.read"], ["crm", "customers", "list"], 1.3, ["crm.modules.customers.keywords"]),
        CreateCrmPage("crm-page-customers-new", "crm.search.pages.customersNew.title", "crm.search.pages.customersNew.summary", "/customers/new", ["crm.customer-management.customers.read"], ["crm", "customers", "new"], 1.2, ["crm.modules.customers.keywords"]),
        CreateCrmPage("crm-page-companies-list", "crm.search.pages.companiesList.title", "crm.search.pages.companiesList.summary", "/companies", ["crm.customer-management.companies.read"], ["crm", "companies", "list"], 1.3, ["crm.modules.companies.keywords"]),
        CreateCrmPage("crm-page-companies-new", "crm.search.pages.companiesNew.title", "crm.search.pages.companiesNew.summary", "/companies/new", ["crm.customer-management.companies.read"], ["crm", "companies", "new"], 1.2, ["crm.modules.companies.keywords"]),
        CreateCrmPage("crm-page-contacts-list", "crm.search.pages.contactsList.title", "crm.search.pages.contactsList.summary", "/contacts", ["crm.customer-management.contacts.read"], ["crm", "contacts", "list"], 1.3, ["crm.modules.contacts.keywords"]),
        CreateCrmPage("crm-page-contacts-new", "crm.search.pages.contactsNew.title", "crm.search.pages.contactsNew.summary", "/contacts/new", ["crm.customer-management.contacts.read"], ["crm", "contacts", "new"], 1.2, ["crm.modules.contacts.keywords"]),
        CreateCrmPage("crm-page-deals-list", "crm.search.pages.dealsList.title", "crm.search.pages.dealsList.summary", "/deals", ["deals.read"], ["crm", "deals", "list"], 1.3, ["crm.modules.deal-management.keywords"]),
        CreateCrmPage("crm-page-deals-new", "crm.search.pages.dealsNew.title", "crm.search.pages.dealsNew.summary", "/deals/new", ["deals.read"], ["crm", "deals", "new"], 1.2, ["crm.modules.deal-management.keywords"]),
        CreateCrmPage("crm-page-tickets-list", "crm.search.pages.ticketsList.title", "crm.search.pages.ticketsList.summary", "/tickets", ["tickets.read"], ["crm", "tickets", "list"], 1.3, ["crm.modules.ticket-management.keywords"]),
        CreateCrmPage("crm-page-tickets-new", "crm.search.pages.ticketsNew.title", "crm.search.pages.ticketsNew.summary", "/tickets/new", ["tickets.read"], ["crm", "tickets", "new"], 1.2, ["crm.modules.ticket-management.keywords"])
    ];

    private StaticSearchDocumentFactory Factory { get; }

    public Task<IReadOnlyCollection<SearchDocument>> GetDocumentsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Factory.CreateDocuments(ManifestItems));
    }

    private static StaticSearchManifestItem CreateCrmModule(
        string id,
        string titleKey,
        string summaryKey,
        string url,
        IReadOnlyCollection<string> requiredPermissions,
        IReadOnlyCollection<string> tags,
        double boost,
        IReadOnlyCollection<string> keywordKeys) =>
        CreateItem(
            id,
            SearchDocumentSource.Crm,
            "module",
            titleKey,
            summaryKey,
            url,
            SearchDocumentVisibility.Permission,
            requiredPermissions,
            tags,
            boost,
            keywordKeys: keywordKeys);

    private static StaticSearchManifestItem CreateCrmPage(
        string id,
        string titleKey,
        string summaryKey,
        string url,
        IReadOnlyCollection<string> requiredPermissions,
        IReadOnlyCollection<string> tags,
        double boost,
        IReadOnlyCollection<string> keywordKeys) =>
        CreateItem(
            id,
            SearchDocumentSource.Crm,
            "page",
            titleKey,
            summaryKey,
            url,
            SearchDocumentVisibility.Permission,
            requiredPermissions,
            tags,
            boost,
            keywordKeys: keywordKeys);

    private static StaticSearchManifestItem CreateItem(
        string id,
        SearchDocumentSource source,
        string type,
        string titleKey,
        string summaryKey,
        string url,
        SearchDocumentVisibility visibility,
        IReadOnlyCollection<string>? requiredPermissions = null,
        IReadOnlyCollection<string>? tags = null,
        double boost = 1,
        string? contentKey = null,
        IReadOnlyCollection<string>? keywordKeys = null,
        SearchPermissionMatchMode permissionMatchMode = SearchPermissionMatchMode.Any,
        IReadOnlyDictionary<string, string>? metadata = null) =>
        new(id, source, type, titleKey, summaryKey, url, visibility)
        {
            RequiredPermissions = requiredPermissions ?? [],
            Tags = tags ?? [],
            Boost = boost,
            ContentKey = contentKey,
            KeywordKeys = keywordKeys ?? [],
            PermissionMatchMode = permissionMatchMode,
            Metadata = metadata ?? new Dictionary<string, string>(StringComparer.Ordinal)
        };
}
