import "server-only";

import type { HttpMethod } from "@/lib/crm-api";
import { CrmApiError, crmApiClient } from "@/lib/crm-api";
import { getCrmApiRequestOptions } from "@/lib/crm-auth/crm-api-request-options";
import { requireCrmSession } from "@/lib/crm-auth/require-crm-session";
import type { CrmSession } from "@/lib/crm-auth/crm-session";

type OperationalQueryValue = string | number | boolean | undefined;
type OperationalQuery = Record<string, OperationalQueryValue>;

type OperationalContext = {
  tenantId: string;
  roleName: string;
  periodStart: string;
  periodEnd: string;
  customerId?: string;
};

export type CrmOperationalEndpointKind = "workspace" | "list" | "detail" | "analytics" | "mutation";

export type CrmOperationalEndpointDefinition = {
  id: string;
  label: string;
  method: HttpMethod;
  path: string | ((context: OperationalContext) => string);
  query?: OperationalQuery | ((context: OperationalContext) => OperationalQuery);
  kind: CrmOperationalEndpointKind;
  fetch?: boolean;
  requiresSelection?: string;
};

export type CrmOperationalModuleConfig = {
  path: string;
  title: string;
  description: string;
  endpoints: CrmOperationalEndpointDefinition[];
  resolveCustomerContext?: boolean;
};

export type CrmOperationalPreview = {
  columns: string[];
  rows: Record<string, string>[];
};

export type CrmOperationalPayloadSummary = {
  kind: "empty" | "collection" | "paged_collection" | "object" | "scalar";
  summary: string;
  count: number | null;
  preview: CrmOperationalPreview;
};

export type CrmOperationalEndpointResult = CrmOperationalEndpointDefinition & {
  resolvedPath: string;
  resolvedQuery: OperationalQuery;
  status: "loaded" | "empty" | "skipped" | "failed";
  statusCode?: number;
  errorMessage?: string;
  payload?: CrmOperationalPayloadSummary;
};

export type CrmOperationalModuleData = {
  config: CrmOperationalModuleConfig;
  endpoints: CrmOperationalEndpointResult[];
  loadedCount: number;
  failedCount: number;
  skippedCount: number;
  mutationCount: number;
};

const pagedQuery = { page: 1, pageSize: 10 } satisfies OperationalQuery;

const operationalModuleConfigs: CrmOperationalModuleConfig[] = [
  {
    path: "/support-inbox",
    title: "Support inbox",
    description:
      "Separate mailbox connections, message triage, synchronization, and routing rules into focused support operations.",
    endpoints: [
      listEndpoint("connections", "Connections", "/api/support-inbox/connections"),
      listEndpoint("messages", "Messages", "/api/support-inbox/messages"),
      {
        id: "create-connection",
        label: "Create connection",
        method: "POST",
        path: "/api/support-inbox/connections",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "create-rule",
        label: "Create routing rule",
        method: "POST",
        path: "/api/support-inbox/rules",
        kind: "mutation",
        fetch: false,
      },
    ],
  },
  {
    path: "/ticket-sla",
    title: "Ticket SLA",
    description:
      "Manage SLA policies, escalation rules, ticket clocks, breach state, and escalation runs from focused views.",
    endpoints: [
      listEndpoint("policies", "SLA policies", "/api/ticket-sla/policies"),
      detailEndpoint(
        "escalation-rules",
        "Escalation rules",
        "/api/ticket-sla/policies/{policyId}/escalation-rules",
        "a policy id",
      ),
      detailEndpoint(
        "ticket-workspace",
        "Ticket SLA workspace",
        "/api/ticket-sla/tickets/{ticketId}/workspace",
        "a ticket id",
      ),
      detailEndpoint(
        "escalation-runs",
        "Escalation runs",
        "/api/ticket-sla/tickets/{ticketId}/escalation-runs",
        "a ticket id",
      ),
    ],
  },
  {
    path: "/ticket-workflows",
    title: "Ticket workflows",
    description:
      "Keep queues, assignment history, status history, and ticket workflow actions in separate operational views.",
    endpoints: [
      listEndpoint("queues", "Queues", "/api/ticket-workflow/queues"),
      detailEndpoint(
        "assignment-history",
        "Assignment history",
        "/api/ticket-workflow/tickets/{ticketId}/assignments",
        "a ticket id",
      ),
      detailEndpoint(
        "status-history",
        "Status history",
        "/api/ticket-workflow/tickets/{ticketId}/status-history",
        "a ticket id",
      ),
    ],
  },
  {
    path: "/customer-intelligence",
    title: "Customer intelligence",
    description:
      "Bring customer health, relationships, identity resolution, and duplicate risk into one review surface.",
    resolveCustomerContext: true,
    endpoints: [
      {
        id: "customer-360-workspace",
        label: "Customer 360 workspace",
        method: "GET",
        path: "/api/customer-intelligence/customers/{customerId}/workspace",
        kind: "workspace",
        requiresSelection: "a customer record",
      },
      {
        id: "customer-portal-summary",
        label: "Customer portal summary",
        method: "GET",
        path: "/api/customer-intelligence/customers/{customerId}/portal-summary",
        kind: "analytics",
        requiresSelection: "a customer record",
        fetch: true,
      },
      {
        id: "detect-duplicates",
        label: "Detect duplicates",
        method: "POST",
        path: "/api/customer-intelligence/duplicates/detect",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "merge-entities",
        label: "Merge entities",
        method: "POST",
        path: "/api/customer-intelligence/merges",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "saved-views",
        label: "Create saved view",
        method: "POST",
        path: "/api/customer-intelligence/saved-views",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "append-activity",
        label: "Append customer activity",
        method: "POST",
        path: "/api/customer-intelligence/activities",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "upsert-relationship",
        label: "Upsert relationship",
        method: "PUT",
        path: "/api/customer-intelligence/relationships",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "track-cdp-event",
        label: "Track CDP event",
        method: "POST",
        path: "/api/customer-intelligence/cdp/events",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "identity-resolution",
        label: "Identity resolution",
        method: "POST",
        path: "/api/customer-intelligence/cdp/identity-resolution",
        kind: "mutation",
        fetch: false,
      },
    ],
  },
  {
    path: "/sales-forecasting",
    title: "Sales forecasting",
    description:
      "Track forecast coverage, quota movement, adjustments, snapshots, and opportunity rollups for the current period.",
    endpoints: [
      {
        id: "forecast-workspace",
        label: "Forecast workspace",
        method: "GET",
        path: "/api/sales-forecasts/workspace",
        query: forecastPeriodQuery,
        kind: "workspace",
      },
      {
        id: "forecast-summary",
        label: "Forecast summary",
        method: "GET",
        path: "/api/sales-forecasts/summary",
        query: forecastPeriodQuery,
        kind: "analytics",
      },
      {
        id: "opportunity-rows",
        label: "Opportunity forecast rows",
        method: "GET",
        path: "/api/sales-forecasts/opportunity-rows",
        query: forecastPeriodQuery,
        kind: "list",
      },
      {
        id: "quotas",
        label: "Sales quotas",
        method: "GET",
        path: "/api/sales-forecasts/quotas",
        query: forecastPeriodQuery,
        kind: "list",
      },
      {
        id: "adjustments",
        label: "Forecast adjustments",
        method: "GET",
        path: "/api/sales-forecasts/adjustments",
        query: forecastPeriodQuery,
        kind: "list",
      },
      {
        id: "snapshots",
        label: "Forecast snapshots",
        method: "GET",
        path: "/api/sales-forecasts/snapshots",
        query: forecastPeriodQuery,
        kind: "list",
      },
      {
        id: "upsert-quota",
        label: "Upsert quota",
        method: "PUT",
        path: "/api/sales-forecasts/quotas",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "create-adjustment",
        label: "Create adjustment",
        method: "POST",
        path: "/api/sales-forecasts/adjustments",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "create-snapshot",
        label: "Create snapshot",
        method: "POST",
        path: "/api/sales-forecasts/snapshots",
        kind: "mutation",
        fetch: false,
      },
    ],
  },
  {
    path: "/marketing",
    title: "Marketing automation",
    description:
      "Coordinate campaigns, audiences, suppressions, journeys, consent, and worker readiness from one cockpit.",
    endpoints: [
      listEndpoint("campaigns", "Campaigns", "/api/marketing/tenants/{tenantId}/campaigns"),
      listEndpoint("segments", "Segments", "/api/marketing/tenants/{tenantId}/segments"),
      listEndpoint(
        "suppressions",
        "Suppressions",
        "/api/marketing/tenants/{tenantId}/suppressions",
      ),
      listEndpoint(
        "email-templates",
        "Email templates",
        "/api/marketing/tenants/{tenantId}/email-templates",
      ),
      listEndpoint("journeys", "Journeys", "/api/marketing/tenants/{tenantId}/journeys"),
      detailEndpoint(
        "campaign-detail",
        "Campaign detail",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}",
        "a campaign id",
      ),
      detailEndpoint(
        "campaign-roi",
        "Campaign ROI",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}/roi",
        "a campaign id",
      ),
      detailEndpoint(
        "email-template-detail",
        "Email template detail",
        "/api/marketing/tenants/{tenantId}/email-templates/{templateId}",
        "an email template id",
      ),
      detailEndpoint(
        "journey-detail",
        "Journey detail",
        "/api/marketing/tenants/{tenantId}/journeys/{journeyId}",
        "a journey id",
      ),
      detailEndpoint(
        "segment-detail",
        "Segment detail",
        "/api/marketing/tenants/{tenantId}/segments/{segmentId}",
        "a segment id",
      ),
      {
        id: "worker-status",
        label: "Worker status",
        method: "GET",
        path: "/api/marketing/tenants/{tenantId}/worker-status",
        kind: "workspace",
      },
      mutationEndpoint(
        "create-campaign",
        "Create campaign",
        "/api/marketing/tenants/{tenantId}/campaigns",
      ),
      mutationEndpoint(
        "update-campaign",
        "Update campaign",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}",
        "PUT",
      ),
      mutationEndpoint(
        "pause-campaign",
        "Pause campaign",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}/pause",
      ),
      mutationEndpoint(
        "resume-campaign",
        "Resume campaign",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}/resume",
      ),
      mutationEndpoint(
        "cancel-campaign",
        "Cancel campaign",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}/cancel",
      ),
      mutationEndpoint(
        "schedule-campaign",
        "Schedule campaign",
        "/api/marketing/tenants/{tenantId}/campaigns/{campaignId}/schedule",
      ),
      mutationEndpoint(
        "preview-template",
        "Preview email template",
        "/api/marketing/tenants/{tenantId}/email-templates/{templateId}/preview",
      ),
      mutationEndpoint(
        "start-journey",
        "Start journey",
        "/api/marketing/tenants/{tenantId}/journeys/{journeyId}/start",
      ),
      mutationEndpoint(
        "pause-journey",
        "Pause journey",
        "/api/marketing/tenants/{tenantId}/journeys/{journeyId}/pause",
      ),
      mutationEndpoint(
        "evaluate-segment",
        "Evaluate segment",
        "/api/marketing/tenants/{tenantId}/segments/{segmentId}/evaluate",
      ),
      mutationEndpoint(
        "add-suppression",
        "Add suppression",
        "/api/marketing/tenants/{tenantId}/suppressions",
      ),
      mutationEndpoint(
        "upsert-consent",
        "Upsert consent",
        "/api/marketing/tenants/{tenantId}/consent",
      ),
      mutationEndpoint(
        "unsubscribe",
        "Unsubscribe",
        "/api/marketing/tenants/{tenantId}/unsubscribe",
      ),
    ],
  },
  {
    path: "/omnichannel",
    title: "Omnichannel",
    description:
      "Monitor channel conversations, assignments, customer links, and priority changes across support channels.",
    endpoints: [
      {
        id: "workspace",
        label: "Omnichannel workspace",
        method: "GET",
        path: "/api/omnichannel/workspace",
        kind: "workspace",
      },
      listEndpoint("conversations", "Conversations", "/api/omnichannel/conversations"),
      detailEndpoint(
        "conversation-detail",
        "Conversation detail",
        "/api/omnichannel/conversations/{conversationId}",
        "a conversation id",
      ),
      detailEndpoint(
        "conversation-messages",
        "Conversation messages",
        "/api/omnichannel/conversations/{conversationId}/messages",
        "a conversation id",
      ),
      mutationEndpoint("create-account", "Create account", "/api/omnichannel/accounts"),
      mutationEndpoint(
        "sync-account",
        "Sync account",
        "/api/omnichannel/accounts/{accountId}/sync",
      ),
      mutationEndpoint(
        "conversation-status",
        "Change conversation status",
        "/api/omnichannel/conversations/{conversationId}/status",
      ),
      mutationEndpoint(
        "conversation-assign",
        "Assign conversation",
        "/api/omnichannel/conversations/{conversationId}/assign",
      ),
      mutationEndpoint(
        "conversation-unassign",
        "Unassign conversation",
        "/api/omnichannel/conversations/{conversationId}/unassign",
      ),
      mutationEndpoint(
        "conversation-priority",
        "Change priority",
        "/api/omnichannel/conversations/{conversationId}/priority",
      ),
      mutationEndpoint(
        "conversation-mark-read",
        "Mark read",
        "/api/omnichannel/conversations/{conversationId}/mark-read",
      ),
      mutationEndpoint(
        "conversation-note",
        "Add conversation note",
        "/api/omnichannel/conversations/{conversationId}/note",
      ),
      mutationEndpoint(
        "link-customer",
        "Link customer",
        "/api/omnichannel/conversations/{conversationId}/link/customer",
      ),
      mutationEndpoint(
        "link-lead",
        "Link lead",
        "/api/omnichannel/conversations/{conversationId}/link/lead",
      ),
    ],
  },
  {
    path: "/calendar-sync",
    title: "Calendar sync",
    description:
      "Review calendar connection health and trigger the sync work that keeps meetings aligned.",
    endpoints: [
      {
        id: "overview",
        label: "Calendar overview",
        method: "GET",
        path: "/api/calendar-sync/overview",
        kind: "workspace",
      },
      {
        id: "upsert-connection",
        label: "Upsert connection",
        method: "PUT",
        path: "/api/calendar-sync/connections/{connectionId}",
        kind: "mutation",
        fetch: false,
      },
      mutationEndpoint(
        "trigger-sync",
        "Trigger sync",
        "/api/calendar-sync/connections/{connectionId}/sync",
      ),
    ],
  },
  {
    path: "/contracts",
    title: "Contract lifecycle",
    description: "Review the contract portfolio and start renewal-ready records for account teams.",
    endpoints: [
      listEndpoint("contracts", "Contracts", "/api/contracts"),
      {
        id: "contract-detail",
        label: "Contract detail",
        method: "GET",
        path: "/api/contracts/{contractId}",
        kind: "detail",
        requiresSelection: "a contract id",
      },
      mutationEndpoint("create-contract", "Create contract", "/api/contracts"),
    ],
  },
  {
    path: "/documents",
    title: "Document management",
    description: "Track document libraries, versions, review state, and approval readiness.",
    endpoints: [
      listEndpoint("documents", "Documents", "/api/documents"),
      {
        id: "document-detail",
        label: "Document detail",
        method: "GET",
        path: "/api/documents/{documentId}",
        kind: "detail",
        requiresSelection: "a document id",
      },
      mutationEndpoint("create-document", "Create document", "/api/documents"),
      mutationEndpoint(
        "create-version",
        "Create document version",
        "/api/documents/{documentId}/versions",
      ),
      mutationEndpoint(
        "create-review",
        "Create document review",
        "/api/documents/{documentId}/reviews",
      ),
    ],
  },
  {
    path: "/finance",
    title: "Finance operations",
    description:
      "Keep sales orders, handoff state, revenue signals, and collection work visible to operators.",
    endpoints: [
      listEndpoint("orders", "Orders", "/api/orders"),
      {
        id: "order-detail",
        label: "Order detail",
        method: "GET",
        path: "/api/orders/{orderId}",
        kind: "detail",
        requiresSelection: "an order id",
      },
      mutationEndpoint("create-order", "Create order", "/api/orders"),
    ],
  },
  {
    path: "/integrations",
    title: "Integration hub",
    description:
      "Watch integration jobs, provider health, webhook readiness, API keys, and dead-letter recovery.",
    endpoints: [
      {
        id: "overview",
        label: "Integration overview",
        method: "GET",
        path: "/api/integrations/tenants/{tenantId}/overview",
        kind: "workspace",
      },
      listEndpoint("jobs", "Integration jobs", "/api/integrations/tenants/{tenantId}/jobs"),
      listEndpoint(
        "dead-letters",
        "Dead letters",
        "/api/integrations/tenants/{tenantId}/dead-letters",
      ),
      {
        id: "connector-health",
        label: "Connector health",
        method: "GET",
        path: "/api/integrations/tenants/{tenantId}/connector-health",
        kind: "analytics",
      },
      {
        id: "worker-status",
        label: "Worker status",
        method: "GET",
        path: "/api/integrations/tenants/{tenantId}/worker-status",
        kind: "workspace",
      },
      listEndpoint("api-keys", "API keys", "/api/integrations/tenants/{tenantId}/api-keys"),
      listEndpoint("webhooks", "Webhooks", "/api/integrations/tenants/{tenantId}/webhooks"),
      {
        id: "mock-provider",
        label: "Mock provider",
        method: "GET",
        path: "/api/integrations/tenants/{tenantId}/providers/mock",
        kind: "detail",
      },
      {
        id: "provider-catalog",
        label: "Provider catalog",
        method: "GET",
        path: "/api/integrations/tenants/{tenantId}/providers/catalog",
        kind: "list",
      },
      listEndpoint(
        "providers",
        "Provider credentials",
        "/api/integrations/tenants/{tenantId}/providers",
      ),
      mutationEndpoint(
        "upsert-connections",
        "Upsert connection",
        "/api/integrations/tenants/{tenantId}/connections",
      ),
      mutationEndpoint(
        "create-job",
        "Create integration job",
        "/api/integrations/tenants/{tenantId}/jobs",
      ),
      mutationEndpoint(
        "create-webhook",
        "Create webhook",
        "/api/integrations/tenants/{tenantId}/webhooks",
      ),
      mutationEndpoint(
        "create-api-key",
        "Create API key",
        "/api/integrations/tenants/{tenantId}/api-keys",
      ),
      mutationEndpoint(
        "webhook-meta",
        "Webhook meta",
        "/api/integrations/webhooks/meta/{endpointKey}",
      ),
      detailEndpoint(
        "webhook-meta-verify",
        "Verify webhook meta",
        "/api/integrations/webhooks/meta/{endpointKey}/verify",
        "an endpoint key",
      ),
      mutationEndpoint(
        "mock-webhook",
        "Mock webhook",
        "/api/integrations/webhooks/mock/{endpointKey}",
      ),
      mutationEndpoint(
        "mock-webhook-reply",
        "Mock conversation reply",
        "/api/integrations/webhooks/mock/conversations/{conversationId}/reply",
      ),
      mutationEndpoint(
        "mock-webhook-create-lead",
        "Mock create lead",
        "/api/integrations/webhooks/mock/conversations/{conversationId}/create-lead",
      ),
    ],
  },
  {
    path: "/knowledge-base",
    title: "Knowledge base",
    description:
      "Manage article coverage, category structure, publishing readiness, and archive flow.",
    endpoints: [
      listEndpoint("articles", "Articles", "/api/knowledge-base/articles"),
      listEndpoint("categories", "Categories", "/api/knowledge-base/categories"),
      {
        id: "article-by-slug",
        label: "Article by slug",
        method: "GET",
        path: "/api/knowledge-base/articles/by-slug/{slug}",
        kind: "detail",
        requiresSelection: "an article slug",
      },
      mutationEndpoint("create-article", "Create article", "/api/knowledge-base/articles"),
      mutationEndpoint("create-category", "Create category", "/api/knowledge-base/categories"),
      mutationEndpoint(
        "update-article",
        "Update article",
        "/api/knowledge-base/articles/{articleId}",
        "PUT",
      ),
      mutationEndpoint(
        "delete-article",
        "Delete article",
        "/api/knowledge-base/articles/{articleId}",
        "DELETE",
      ),
      mutationEndpoint(
        "update-category",
        "Update category",
        "/api/knowledge-base/categories/{categoryId}",
        "PUT",
      ),
      mutationEndpoint(
        "publish-article",
        "Publish article",
        "/api/knowledge-base/articles/{articleId}/publish",
      ),
      mutationEndpoint(
        "archive-article",
        "Archive article",
        "/api/knowledge-base/articles/{articleId}/archive",
      ),
    ],
  },
  {
    path: "/work-management",
    title: "Work management",
    description:
      "Coordinate tasks, meetings, activities, and workload signals for customer-facing teams.",
    endpoints: [
      {
        id: "workspace",
        label: "Work management workspace",
        method: "GET",
        path: "/api/work-management/workspace",
        kind: "workspace",
      },
      mutationEndpoint("create-task", "Create task", "/api/work-management/tasks"),
      mutationEndpoint("schedule-meeting", "Schedule meeting", "/api/work-management/meetings"),
    ],
  },
  {
    path: "/workflows",
    title: "Workflow automation",
    description:
      "Review automation rules, executions, worker state, approvals, and assignment logic.",
    endpoints: [
      listEndpoint("rules", "Workflow rules", "/api/workflows/tenants/{tenantId}/rules"),
      listEndpoint(
        "executions",
        "Workflow executions",
        "/api/workflows/tenants/{tenantId}/executions",
      ),
      detailEndpoint(
        "rule-detail",
        "Workflow rule detail",
        "/api/workflows/tenants/{tenantId}/rules/{ruleId}",
        "a rule id",
      ),
      detailEndpoint(
        "execution-detail",
        "Workflow execution detail",
        "/api/workflows/tenants/{tenantId}/executions/{executionLogId}",
        "an execution log id",
      ),
      {
        id: "worker-status",
        label: "Worker status",
        method: "GET",
        path: "/api/workflows/tenants/{tenantId}/worker-status",
        kind: "workspace",
      },
      mutationEndpoint("create-rule", "Create rule", "/api/workflows/tenants/{tenantId}/rules"),
      mutationEndpoint(
        "activate-rule",
        "Activate rule",
        "/api/workflows/tenants/{tenantId}/rules/{ruleId}/activate",
      ),
      mutationEndpoint(
        "deactivate-rule",
        "Deactivate rule",
        "/api/workflows/tenants/{tenantId}/rules/{ruleId}/deactivate",
      ),
      mutationEndpoint(
        "evaluate-rules",
        "Evaluate rules",
        "/api/workflows/tenants/{tenantId}/rules/evaluate",
      ),
      mutationEndpoint(
        "dry-run",
        "Dry run rules",
        "/api/workflows/tenants/{tenantId}/rules/dry-run",
      ),
      mutationEndpoint(
        "retry-execution",
        "Retry execution",
        "/api/workflows/tenants/{tenantId}/executions/{executionLogId}/retry",
      ),
      mutationEndpoint("create-approval", "Create approval", "/api/workflows/approvals"),
      mutationEndpoint(
        "create-assignment-rule",
        "Create assignment rule",
        "/api/workflows/assignment-rules",
      ),
    ],
  },
  {
    path: "/analytics",
    title: "Analytics reporting",
    description:
      "Scan funnel, ROI, revenue aging, support KPI, productivity, and role dashboard signals.",
    endpoints: [
      getTenantAnalyticsEndpoint("summary", "Tenant summary", "summary"),
      getTenantAnalyticsEndpoint("sales-funnel", "Sales funnel", "sales-funnel"),
      getTenantAnalyticsEndpoint("campaign-roi", "Campaign ROI", "campaign-roi"),
      getTenantAnalyticsEndpoint("revenue-aging", "Revenue aging", "revenue-aging"),
      getTenantAnalyticsEndpoint("support-kpis", "Support KPIs", "support-kpis"),
      getTenantAnalyticsEndpoint("user-productivity", "User productivity", "user-productivity"),
      getTenantAnalyticsEndpoint("projection-status", "Projection status", "projection-status"),
      {
        id: "role-dashboard",
        label: "Role dashboard",
        method: "GET",
        path: (context) =>
          `/api/analytics/dashboards/roles/${encodeURIComponent(context.roleName)}`,
        kind: "workspace",
      },
    ],
  },
  {
    path: "/ai",
    title: "Artificial intelligence",
    description:
      "Inspect AI provider readiness and the operational workspace that powers assisted workflows.",
    endpoints: [
      {
        id: "workspace",
        label: "AI workspace",
        method: "GET",
        path: "/api/artificial-intelligence/workspace",
        kind: "workspace",
      },
      {
        id: "upsert-provider",
        label: "Upsert provider",
        method: "PUT",
        path: "/api/artificial-intelligence/providers/{providerId}",
        kind: "mutation",
        fetch: false,
      },
    ],
  },
  {
    path: "/tags",
    title: "Tags",
    description:
      "Govern tags, tag groups, smart labels, and classification schemes used across CRM records.",
    endpoints: [
      listEndpoint("tags", "Tags", "/api/tags"),
      listEndpoint("tag-groups", "Tag groups", "/api/tags/groups"),
      listEndpoint("smart-label-rules", "Smart label rules", "/api/tags/smart-label-rules"),
      listEndpoint(
        "classification-schemes",
        "Classification schemes",
        "/api/tags/classification-schemes",
      ),
      mutationEndpoint("create-tag", "Create tag", "/api/tags"),
      mutationEndpoint("create-tag-group", "Create tag group", "/api/tags/groups"),
      mutationEndpoint(
        "create-smart-label-rule",
        "Create smart label rule",
        "/api/tags/smart-label-rules",
      ),
      mutationEndpoint(
        "create-classification-scheme",
        "Create classification scheme",
        "/api/tags/classification-schemes",
      ),
    ],
  },
  {
    path: "/tenants",
    title: "Tenants",
    description:
      "Review tenant configuration, branding readiness, feature flags, and module toggles.",
    endpoints: [
      {
        id: "summary",
        label: "Tenant summary",
        method: "GET",
        path: "/api/tenants/{tenantId}/summary",
        kind: "workspace",
      },
      mutationEndpoint("create-tenant", "Create tenant", "/api/tenants"),
      {
        id: "branding",
        label: "Update branding",
        method: "PUT",
        path: "/api/tenants/{tenantId}/branding",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "feature-flag",
        label: "Update feature flag",
        method: "PUT",
        path: "/api/tenants/{tenantId}/feature-flags/{key}",
        kind: "mutation",
        fetch: false,
      },
      {
        id: "module-toggle",
        label: "Update module toggle",
        method: "PUT",
        path: "/api/tenants/{tenantId}/modules/{moduleKey}",
        kind: "mutation",
        fetch: false,
      },
    ],
  },
  {
    path: "/activities",
    title: "Activities",
    description:
      "See activity-facing work from the same operational workspace that powers tasks and meetings.",
    endpoints: [
      {
        id: "work-management-workspace",
        label: "Work management workspace",
        method: "GET",
        path: "/api/work-management/workspace",
        kind: "workspace",
      },
      mutationEndpoint("create-task", "Create task", "/api/work-management/tasks"),
      mutationEndpoint("schedule-meeting", "Schedule meeting", "/api/work-management/meetings"),
    ],
  },
];

export function getOperationalModuleConfig(path: string): CrmOperationalModuleConfig | null {
  return operationalModuleConfigs.find((config) => config.path === path) ?? null;
}

export async function getOperationalModuleData(
  path: string,
): Promise<CrmOperationalModuleData | null> {
  const config = getOperationalModuleConfig(path);
  if (!config) {
    return null;
  }

  const options = await getCrmApiRequestOptions();
  const session = await requireCrmSession("/");
  const context = await createOperationalContext(session, config);

  const endpoints = await Promise.all(
    config.endpoints.map((endpoint) => resolveOperationalEndpoint(endpoint, context, options)),
  );

  return {
    config,
    endpoints,
    loadedCount: endpoints.filter((endpoint) => endpoint.status === "loaded").length,
    failedCount: endpoints.filter((endpoint) => endpoint.status === "failed").length,
    skippedCount: endpoints.filter((endpoint) => endpoint.status === "skipped").length,
    mutationCount: endpoints.filter((endpoint) => endpoint.kind === "mutation").length,
  };
}

export function summarizeOperationalPayload(payload: unknown): CrmOperationalPayloadSummary {
  const collection = getPayloadCollection(payload);

  if (collection) {
    const preview = createCollectionPreview(collection.items);
    const count = collection.totalCount ?? collection.items.length;
    return {
      kind: collection.isPaged ? "paged_collection" : "collection",
      summary: `${count} records`,
      count,
      preview,
    };
  }

  if (isRecord(payload)) {
    const entries = Object.entries(payload);
    const primitiveRows = entries.slice(0, 10).map(([field, value]) => ({
      field,
      value: stringifyDisplayValue(value),
    }));

    return {
      kind: entries.length === 0 ? "empty" : "object",
      summary: entries.length === 0 ? "No fields returned" : `${entries.length} fields`,
      count: entries.length,
      preview: {
        columns: ["field", "value"],
        rows: primitiveRows,
      },
    };
  }

  if (payload === null || payload === undefined || payload === "") {
    return {
      kind: "empty",
      summary: "No payload returned",
      count: 0,
      preview: { columns: [], rows: [] },
    };
  }

  return {
    kind: "scalar",
    summary: stringifyDisplayValue(payload),
    count: null,
    preview: {
      columns: ["value"],
      rows: [{ value: stringifyDisplayValue(payload) }],
    },
  };
}

async function resolveOperationalEndpoint(
  endpoint: CrmOperationalEndpointDefinition,
  context: OperationalContext,
  options: Awaited<ReturnType<typeof getCrmApiRequestOptions>>,
): Promise<CrmOperationalEndpointResult> {
  const resolvedPath = resolvePath(endpoint.path, context);
  const resolvedQuery = resolveQuery(endpoint.query, context);
  const baseResult = {
    ...endpoint,
    resolvedPath,
    resolvedQuery,
  };

  if (endpoint.fetch === false || endpoint.method !== "GET") {
    return {
      ...baseResult,
      status: "skipped",
      errorMessage:
        "Mutation endpoint is registered for coverage and awaits an explicit form action.",
    };
  }

  if (containsUnresolvedPlaceholder(resolvedPath)) {
    return {
      ...baseResult,
      status: "skipped",
      errorMessage: `Requires ${endpoint.requiresSelection ?? "a selected record"}.`,
    };
  }

  try {
    const payload = await crmApiClient.fetchOperationalEndpoint(
      resolvedPath,
      resolvedQuery,
      options,
    );
    const summary = summarizeOperationalPayload(payload);

    return {
      ...baseResult,
      status: summary.kind === "empty" || summary.count === 0 ? "empty" : "loaded",
      payload: summary,
    };
  } catch (error) {
    return {
      ...baseResult,
      status: "failed",
      ...(error instanceof CrmApiError
        ? {
            statusCode: error.status,
            errorMessage: error.message,
          }
        : {
            errorMessage: "CRM API request failed.",
          }),
    };
  }
}

async function createOperationalContext(
  session: CrmSession,
  config: CrmOperationalModuleConfig,
): Promise<OperationalContext> {
  const forecastRange = getCurrentMonthForecastRange();
  const baseContext: OperationalContext = {
    tenantId: session.profile.tenantId,
    roleName: session.profile.roles[0] ?? "crm-user",
    periodStart: forecastRange.periodStart,
    periodEnd: forecastRange.periodEnd,
  };

  if (!config.resolveCustomerContext) {
    return baseContext;
  }

  const customerId = await resolveFirstCustomerId();
  return customerId ? { ...baseContext, customerId } : baseContext;
}

async function resolveFirstCustomerId(): Promise<string | undefined> {
  try {
    const options = await getCrmApiRequestOptions();
    const customers = await crmApiClient.listCustomers({ page: 1, pageSize: 1 }, options);
    return customers.items[0]?.id;
  } catch {
    return undefined;
  }
}

function getCurrentMonthForecastRange(now = new Date()): {
  periodStart: string;
  periodEnd: string;
} {
  const start = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), 1));
  const end = new Date(Date.UTC(now.getUTCFullYear(), now.getUTCMonth() + 1, 0));

  return {
    periodStart: start.toISOString().slice(0, 10),
    periodEnd: end.toISOString().slice(0, 10),
  };
}

function forecastPeriodQuery(context: OperationalContext): OperationalQuery {
  return {
    periodStart: context.periodStart,
    periodEnd: context.periodEnd,
  };
}

function listEndpoint(id: string, label: string, path: string): CrmOperationalEndpointDefinition {
  return {
    id,
    label,
    method: "GET",
    path,
    query: pagedQuery,
    kind: "list",
  };
}

function detailEndpoint(
  id: string,
  label: string,
  path: string,
  requiresSelection: string,
): CrmOperationalEndpointDefinition {
  return {
    id,
    label,
    method: "GET",
    path,
    kind: "detail",
    requiresSelection,
  };
}

function mutationEndpoint(
  id: string,
  label: string,
  path: string,
  method: HttpMethod = "POST",
): CrmOperationalEndpointDefinition {
  return {
    id,
    label,
    method,
    path,
    kind: "mutation",
    fetch: false,
  };
}

function getTenantAnalyticsEndpoint(
  id: string,
  label: string,
  segment: string,
): CrmOperationalEndpointDefinition {
  return {
    id,
    label,
    method: "GET",
    path: `/api/analytics/tenants/{tenantId}/${segment}`,
    kind: "analytics",
  };
}

function resolvePath(
  path: CrmOperationalEndpointDefinition["path"],
  context: OperationalContext,
): string {
  const rawPath = typeof path === "function" ? path(context) : path;

  return rawPath
    .replaceAll("{tenantId}", context.tenantId)
    .replaceAll("{roleName}", encodeURIComponent(context.roleName))
    .replaceAll("{customerId}", context.customerId ?? "{customerId}");
}

function resolveQuery(
  query: CrmOperationalEndpointDefinition["query"],
  context: OperationalContext,
): OperationalQuery {
  if (!query) {
    return {};
  }

  return typeof query === "function" ? query(context) : query;
}

function containsUnresolvedPlaceholder(path: string): boolean {
  return /\{[A-Za-z0-9]+\}/.test(path);
}

function getPayloadCollection(payload: unknown): {
  items: unknown[];
  totalCount?: number;
  isPaged: boolean;
} | null {
  if (Array.isArray(payload)) {
    return { items: payload, isPaged: false };
  }

  if (!isRecord(payload)) {
    return null;
  }

  const items = Array.isArray(payload.items)
    ? payload.items
    : Array.isArray(payload.results)
      ? payload.results
      : Array.isArray(payload.data)
        ? payload.data
        : null;

  if (!items) {
    return null;
  }

  const collection = {
    items,
    isPaged: true,
  };

  return typeof payload.totalCount === "number"
    ? { ...collection, totalCount: payload.totalCount }
    : collection;
}

function createCollectionPreview(items: unknown[]): CrmOperationalPreview {
  const records = items.filter(isRecord).slice(0, 5);
  if (records.length === 0) {
    return { columns: [], rows: [] };
  }

  const preferredColumns = [
    "id",
    "code",
    "name",
    "title",
    "subject",
    "status",
    "type",
    "provider",
    "isActive",
    "createdAt",
    "updatedAt",
  ];
  const discoveredColumns = new Set<string>();

  for (const column of preferredColumns) {
    if (records.some((record) => column in record)) {
      discoveredColumns.add(column);
    }
  }

  for (const record of records) {
    for (const key of Object.keys(record)) {
      if (discoveredColumns.size >= 6) {
        break;
      }
      discoveredColumns.add(key);
    }
  }

  const columns = [...discoveredColumns].slice(0, 6);
  return {
    columns,
    rows: records.map((record) =>
      Object.fromEntries(columns.map((column) => [column, stringifyDisplayValue(record[column])])),
    ),
  };
}

function stringifyDisplayValue(value: unknown): string {
  if (value === null || value === undefined) {
    return "-";
  }

  if (typeof value === "string") {
    return value.length > 96 ? `${value.slice(0, 93)}...` : value;
  }

  if (typeof value === "number" || typeof value === "boolean") {
    return String(value);
  }

  if (Array.isArray(value)) {
    return `${value.length} items`;
  }

  if (isRecord(value)) {
    const label = getRecordLabel(value);
    return label ?? `${Object.keys(value).length} fields`;
  }

  return String(value);
}

function getRecordLabel(value: Record<string, unknown>): string | null {
  for (const key of ["name", "title", "subject", "code", "id"]) {
    const candidate = value[key];
    if (typeof candidate === "string" && candidate.trim().length > 0) {
      return candidate;
    }
  }

  return null;
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return Boolean(value) && typeof value === "object" && !Array.isArray(value);
}
