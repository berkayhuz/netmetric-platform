import type {
  DashboardProfile,
  DashboardWidgetInstance,
  WidgetCategory,
  WidgetType,
} from "@/features/widgets/types";

export type WidgetRenderKind = "custom" | "metric" | "list" | "status";

export type WidgetCatalogItem = {
  type: WidgetType;
  title: string;
  description: string;
  category: WidgetCategory;
  renderKind: WidgetRenderKind;
  dataKey?: string;
  defaultSize: { w: number; h: number };
  minSize: { w: number; h: number };
  maxSize: { w: number; h: number };
  defaultRefresh: { enabled: boolean; intervalSec: 15 | 30 | 60 | null };
};

export type CrmWidgetMetricKind =
  | "total"
  | "active"
  | "inactive"
  | "open"
  | "closed"
  | "assigned"
  | "unassigned"
  | "highPriority"
  | "overdue"
  | "dueToday"
  | "amountTotal"
  | "expectedRevenue"
  | "probabilityAverage"
  | "recentList"
  | "statusBreakdown"
  | "priorityBreakdown";

export type CrmGeneratedWidgetDefinition = {
  id: WidgetType;
  source: string;
  metric: CrmWidgetMetricKind;
  title: string;
  description: string;
  category: WidgetCategory;
  renderKind: WidgetRenderKind;
  path: string;
};

const DEFAULT_REFRESH = { enabled: true, intervalSec: 30 as const };

const customWidgetCatalog: WidgetCatalogItem[] = [
  {
    type: "customers_kpi",
    title: "Total Customers",
    description: "Current number of customers",
    category: "core",
    renderKind: "custom",
    defaultSize: { w: 3, h: 4 },
    minSize: { w: 2, h: 4 },
    maxSize: { w: 3, h: 5 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "companies_kpi",
    title: "Total Companies",
    description: "Current number of companies",
    category: "core",
    renderKind: "custom",
    defaultSize: { w: 3, h: 4 },
    minSize: { w: 2, h: 4 },
    maxSize: { w: 3, h: 5 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "contacts_kpi",
    title: "Total Contacts",
    description: "Current number of contacts",
    category: "core",
    renderKind: "custom",
    defaultSize: { w: 3, h: 4 },
    minSize: { w: 2, h: 4 },
    maxSize: { w: 3, h: 5 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "deals_pipeline_summary",
    title: "Deals Pipeline",
    description: "Stage progress and pipeline momentum",
    category: "sales",
    renderKind: "custom",
    defaultSize: { w: 5, h: 4 },
    minSize: { w: 3, h: 6 },
    maxSize: { w: 5, h: 6 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "opportunities_summary",
    title: "Opportunities",
    description: "Open opportunity totals and trend",
    category: "sales",
    renderKind: "custom",
    defaultSize: { w: 4, h: 4 },
    minSize: { w: 3, h: 4 },
    maxSize: { w: 4, h: 8 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "tasks_due_today",
    title: "Tasks Due Today",
    description: "Priority tasks you should complete today",
    category: "operations",
    renderKind: "custom",
    defaultSize: { w: 6, h: 4 },
    minSize: { w: 3, h: 4 },
    maxSize: { w: 12, h: 6 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "ticket_sla_risk",
    title: "SLA Risk Alerts",
    description: "Tickets close to SLA breach",
    category: "service_support",
    renderKind: "custom",
    defaultSize: { w: 6, h: 5 },
    minSize: { w: 3, h: 4 },
    maxSize: { w: 12, h: 8 },
    defaultRefresh: { enabled: true, intervalSec: 15 },
  },
  {
    type: "recent_activities_feed",
    title: "Recent Activity",
    description: "Latest activities across CRM modules",
    category: "operations",
    renderKind: "custom",
    defaultSize: { w: 6, h: 5 },
    minSize: { w: 3, h: 4 },
    maxSize: { w: 12, h: 8 },
    defaultRefresh: DEFAULT_REFRESH,
  },
  {
    type: "forecast_snapshot",
    title: "Sales Forecast",
    description: "Forecast overview for upcoming periods",
    category: "sales",
    renderKind: "custom",
    defaultSize: { w: 6, h: 4 },
    minSize: { w: 2, h: 3 },
    maxSize: { w: 12, h: 6 },
    defaultRefresh: { enabled: true, intervalSec: 60 },
  },
  {
    type: "quick_actions",
    title: "Quick Actions",
    description: "One-click shortcuts for common workflows",
    category: "custom",
    renderKind: "custom",
    defaultSize: { w: 4, h: 5 },
    minSize: { w: 2, h: 4 },
    maxSize: { w: 6, h: 8 },
    defaultRefresh: { enabled: false, intervalSec: null },
  },
];

const metricLabels: Record<
  CrmWidgetMetricKind,
  { title: string; description: string; renderKind: WidgetRenderKind }
> = {
  total: { title: "Total", description: "Total records from CRM API", renderKind: "metric" },
  active: { title: "Active", description: "Active records from CRM API", renderKind: "metric" },
  inactive: {
    title: "Inactive",
    description: "Inactive records from CRM API",
    renderKind: "metric",
  },
  open: { title: "Open", description: "Open records from CRM API", renderKind: "metric" },
  closed: { title: "Closed", description: "Closed records from CRM API", renderKind: "metric" },
  assigned: {
    title: "Assigned",
    description: "Records with an owner or assignee",
    renderKind: "metric",
  },
  unassigned: {
    title: "Unassigned",
    description: "Records without an owner or assignee",
    renderKind: "metric",
  },
  highPriority: {
    title: "High Priority",
    description: "High priority records from CRM API",
    renderKind: "metric",
  },
  overdue: { title: "Overdue", description: "Records past due", renderKind: "metric" },
  dueToday: { title: "Due Today", description: "Records due today", renderKind: "metric" },
  amountTotal: { title: "Value", description: "Total monetary value", renderKind: "metric" },
  expectedRevenue: {
    title: "Expected Revenue",
    description: "Weighted or expected revenue",
    renderKind: "metric",
  },
  probabilityAverage: {
    title: "Avg Probability",
    description: "Average probability",
    renderKind: "metric",
  },
  recentList: { title: "Recent", description: "Recent records from CRM API", renderKind: "list" },
  statusBreakdown: {
    title: "Status Mix",
    description: "Status distribution",
    renderKind: "status",
  },
  priorityBreakdown: {
    title: "Priority Mix",
    description: "Priority distribution",
    renderKind: "status",
  },
};

type CrmWidgetSourceDefinition = {
  source: string;
  title: string;
  category: WidgetCategory;
  path: string;
  metrics: CrmWidgetMetricKind[];
};

export const crmWidgetSources: CrmWidgetSourceDefinition[] = [
  {
    source: "customers",
    title: "Customers",
    category: "core",
    path: "/customers",
    metrics: ["total", "active", "inactive", "assigned", "unassigned", "recentList"],
  },
  {
    source: "companies",
    title: "Companies",
    category: "core",
    path: "/companies",
    metrics: ["total", "active", "inactive", "assigned", "unassigned", "recentList"],
  },
  {
    source: "contacts",
    title: "Contacts",
    category: "core",
    path: "/contacts",
    metrics: ["total", "active", "inactive", "assigned", "unassigned", "recentList"],
  },
  {
    source: "leads",
    title: "Leads",
    category: "sales",
    path: "/leads",
    metrics: [
      "total",
      "active",
      "inactive",
      "assigned",
      "unassigned",
      "highPriority",
      "dueToday",
      "overdue",
      "statusBreakdown",
      "priorityBreakdown",
      "recentList",
    ],
  },
  {
    source: "opportunities",
    title: "Opportunities",
    category: "sales",
    path: "/opportunities",
    metrics: [
      "total",
      "active",
      "inactive",
      "assigned",
      "unassigned",
      "open",
      "closed",
      "amountTotal",
      "expectedRevenue",
      "probabilityAverage",
      "statusBreakdown",
      "priorityBreakdown",
      "recentList",
    ],
  },
  {
    source: "deals",
    title: "Deals",
    category: "sales",
    path: "/deals",
    metrics: [
      "total",
      "active",
      "inactive",
      "assigned",
      "unassigned",
      "open",
      "closed",
      "amountTotal",
      "statusBreakdown",
      "recentList",
    ],
  },
  {
    source: "quotes",
    title: "Quotes",
    category: "sales",
    path: "/quotes",
    metrics: ["total", "open", "closed", "amountTotal", "statusBreakdown", "recentList"],
  },
  {
    source: "pipelines",
    title: "Pipelines",
    category: "sales",
    path: "/pipeline",
    metrics: ["total", "recentList"],
  },
  {
    source: "proposal_templates",
    title: "Proposal Templates",
    category: "sales",
    path: "/quotes",
    metrics: ["total", "active", "inactive", "recentList"],
  },
  {
    source: "product_rules",
    title: "Product Rules",
    category: "sales",
    path: "/quotes",
    metrics: ["total", "active", "inactive", "priorityBreakdown", "recentList"],
  },
  {
    source: "product_bundles",
    title: "Product Bundles",
    category: "sales",
    path: "/quotes",
    metrics: ["total", "active", "inactive", "amountTotal", "recentList"],
  },
  {
    source: "guided_selling_playbooks",
    title: "Guided Selling Playbooks",
    category: "sales",
    path: "/quotes",
    metrics: ["total", "active", "inactive", "amountTotal", "recentList"],
  },
  {
    source: "deal_lost_reasons",
    title: "Deal Lost Reasons",
    category: "sales",
    path: "/deals",
    metrics: ["total", "recentList"],
  },
  {
    source: "opportunity_lost_reasons",
    title: "Opportunity Lost Reasons",
    category: "sales",
    path: "/opportunities",
    metrics: ["total", "recentList"],
  },
  {
    source: "products",
    title: "Products",
    category: "sales",
    path: "/product-catalog",
    metrics: ["total", "active", "inactive", "amountTotal", "statusBreakdown", "recentList"],
  },
  {
    source: "product_categories",
    title: "Product Categories",
    category: "sales",
    path: "/product-catalog/categories",
    metrics: ["total", "active", "inactive", "recentList"],
  },
  {
    source: "tickets",
    title: "Tickets",
    category: "service_support",
    path: "/tickets",
    metrics: [
      "total",
      "active",
      "inactive",
      "assigned",
      "unassigned",
      "open",
      "closed",
      "highPriority",
      "statusBreakdown",
      "priorityBreakdown",
      "recentList",
    ],
  },
  {
    source: "support_connections",
    title: "Inbox Connections",
    category: "service_support",
    path: "/support-inbox",
    metrics: ["total", "active", "inactive", "recentList"],
  },
  {
    source: "support_messages",
    title: "Inbox Messages",
    category: "service_support",
    path: "/support-inbox",
    metrics: ["total", "open", "closed", "statusBreakdown", "recentList"],
  },
  {
    source: "sla_policies",
    title: "SLA Policies",
    category: "service_support",
    path: "/ticket-sla",
    metrics: ["total", "priorityBreakdown", "recentList"],
  },
  {
    source: "sla_escalation_rules",
    title: "SLA Escalation Rules",
    category: "service_support",
    path: "/ticket-sla",
    metrics: ["total", "active", "inactive", "recentList"],
  },
  {
    source: "workflow_queues",
    title: "Workflow Queues",
    category: "service_support",
    path: "/ticket-workflows",
    metrics: ["total", "recentList"],
  },
  {
    source: "tasks",
    title: "Tasks",
    category: "operations",
    path: "/tasks",
    metrics: [
      "total",
      "assigned",
      "unassigned",
      "open",
      "closed",
      "highPriority",
      "dueToday",
      "overdue",
      "statusBreakdown",
      "priorityBreakdown",
      "recentList",
    ],
  },
  {
    source: "activities",
    title: "Activities",
    category: "operations",
    path: "/activities",
    metrics: ["total", "statusBreakdown", "recentList"],
  },
  {
    source: "work_management",
    title: "Work Management",
    category: "operations",
    path: "/work-management",
    metrics: ["total", "open", "closed", "dueToday", "overdue", "statusBreakdown", "recentList"],
  },
  {
    source: "contracts",
    title: "Contracts",
    category: "operations",
    path: "/contracts",
    metrics: ["total", "active", "inactive", "amountTotal", "statusBreakdown", "recentList"],
  },
  {
    source: "orders",
    title: "Orders",
    category: "operations",
    path: "/finance",
    metrics: ["total", "open", "closed", "amountTotal", "statusBreakdown", "recentList"],
  },
  {
    source: "tags",
    title: "Tags",
    category: "administration",
    path: "/tags",
    metrics: ["total", "recentList"],
  },
  {
    source: "tag_groups",
    title: "Tag Groups",
    category: "administration",
    path: "/tags",
    metrics: ["total", "recentList"],
  },
  {
    source: "smart_label_rules",
    title: "Smart Label Rules",
    category: "administration",
    path: "/tags",
    metrics: ["total", "recentList"],
  },
  {
    source: "classification_schemes",
    title: "Classification Schemes",
    category: "administration",
    path: "/tags",
    metrics: ["total", "recentList"],
  },
];

export const crmGeneratedWidgetDefinitions: CrmGeneratedWidgetDefinition[] =
  crmWidgetSources.flatMap((source) =>
    source.metrics.map((metric) => {
      const label = metricLabels[metric];
      return {
        id: `crm_${source.source}_${metric}`,
        source: source.source,
        metric,
        title: `${source.title} ${label.title}`,
        description: `${source.title}: ${label.description.toLowerCase()}`,
        category: source.category,
        renderKind: label.renderKind,
        path: source.path,
      };
    }),
  );

const generatedWidgetCatalog: WidgetCatalogItem[] = crmGeneratedWidgetDefinitions.map(
  (definition) => ({
    type: definition.id,
    title: definition.title,
    description: definition.description,
    category: definition.category,
    renderKind: definition.renderKind,
    dataKey: definition.id,
    defaultSize: definition.renderKind === "metric" ? { w: 3, h: 4 } : { w: 4, h: 5 },
    minSize: definition.renderKind === "metric" ? { w: 2, h: 3 } : { w: 3, h: 4 },
    maxSize: definition.renderKind === "metric" ? { w: 4, h: 5 } : { w: 8, h: 8 },
    defaultRefresh: DEFAULT_REFRESH,
  }),
);

export const widgetCatalog: WidgetCatalogItem[] = [
  ...customWidgetCatalog,
  ...generatedWidgetCatalog,
];

export function getWidgetCatalogItem(type: WidgetType): WidgetCatalogItem {
  const item = widgetCatalog.find((candidate) => candidate.type === type);
  if (!item) {
    throw new Error(`Unknown widget type: ${type}`);
  }

  return item;
}

function createWidgetInstance(type: WidgetType, index: number): DashboardWidgetInstance {
  const widget = getWidgetCatalogItem(type);
  return {
    id: `${type}_${index + 1}`,
    widgetType: type,
    title: widget.title,
    layout: {
      x: (index * 3) % 12,
      y: Math.floor(index / 4) * 3,
      w: widget.defaultSize.w,
      h: widget.defaultSize.h,
      minW: widget.minSize.w,
      minH: widget.minSize.h,
      maxW: widget.maxSize.w,
      maxH: widget.maxSize.h,
    },
    config: {},
    refreshPolicy: {
      enabled: widget.defaultRefresh.enabled,
      intervalSec: widget.defaultRefresh.intervalSec,
    },
  };
}

export function createDefaultDashboardProfile(): DashboardProfile {
  const widgetTypes: WidgetType[] = [
    "customers_kpi",
    "companies_kpi",
    "contacts_kpi",
    "quick_actions",
  ];

  return {
    id: "dashboard_default",
    name: "Dashboard",
    isDefault: true,
    layoutVersion: 1,
    widgets: widgetTypes.map((type, index) => createWidgetInstance(type, index)),
    shares: [],
    deletedAt: null,
    purgeAt: null,
  };
}
