// <copyright file="CrmModuleCatalog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.API.Configuration;

public sealed record CrmModuleCatalogItem(string Name, string Area, string HealthCheckTag);

public static class CrmModuleCatalog
{
    public static readonly IReadOnlyList<CrmModuleCatalogItem> Modules =
    [
        new("Analytics Reporting", "analytics-reporting", "analytics-reporting"),
        new("Artificial Intelligence", "artificial-intelligence", "artificial-intelligence"),
        new("Calendar Sync", "calendar-sync", "calendar-sync"),
        new("Contract Lifecycle", "contract-lifecycle", "contract-lifecycle"),
        new("Customer Intelligence", "customer-intelligence", "customer-intelligence"),
        new("Customer Management", "customer-management", "customer-management"),
        new("Deal Management", "deal-management", "deal-management"),
        new("Document Management", "document-management", "document-management"),
        new("Finance Operations", "finance-operations", "finance-operations"),
        new("Integration Hub", "integration-hub", "integration-hub"),
        new("Knowledge Base Management", "knowledge-base-management", "knowledge-base-management"),
        new("Lead Management", "lead-management", "lead-management"),
        new("Marketing Automation", "marketing-automation", "marketing-automation"),
        new("Omnichannel", "omnichannel", "omnichannel"),
        new("Opportunity Management", "opportunity-management", "opportunity-management"),
        new("Pipeline Management", "pipeline-management", "pipeline-management"),
        new("Product Catalog", "product-catalog", "product-catalog"),
        new("Quote Management", "quote-management", "quote-management"),
        new("Sales Forecasting", "sales-forecasting", "sales-forecasting"),
        new("Support Inbox Integration", "support-inbox-integration", "support-inbox-integration"),
        new("Tag Management", "tag-management", "tag-management"),
        new("Tenant Management", "tenant-management", "tenant-management"),
        new("Ticket Management", "ticket-management", "ticket-management"),
        new("Ticket SLA Management", "ticket-sla-management", "ticket-sla-management"),
        new("Ticket Workflow Management", "ticket-workflow-management", "ticket-workflow-management"),
        new("Workflow Automation", "workflow-automation", "workflow-automation"),
        new("Work Management", "work-management", "work-management")
    ];
}
