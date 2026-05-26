import type { CrmCapabilities, CrmCapability } from "@/lib/crm-auth/crm-capabilities";
import { crmCapabilityAllows } from "@/lib/crm-auth/crm-capabilities";

export type CrmModuleStatus = "active" | "read_only" | "contract_pending" | "coming_soon";
export type CrmModuleGroup =
  | "core"
  | "sales"
  | "service_support"
  | "marketing"
  | "operations"
  | "intelligence_ai"
  | "administration";

type CrmModuleTextKey = `crm.modules.${string}.${"title" | "description" | "implementationPhase"}`;
export const crmModuleIconKeys = [
  "layout-dashboard",
  "users",
  "building-2",
  "contact",
  "sparkles",
  "user-plus",
  "handshake",
  "target",
  "git-branch",
  "file-text",
  "trending-up",
  "package",
  "inbox",
  "ticket",
  "alarm-clock",
  "shuffle",
  "megaphone",
  "messages-square",
  "calendar",
  "file-signature",
  "folder-open",
  "wallet",
  "plug",
  "book-open",
  "list-checks",
  "workflow",
  "bar-chart-3",
  "bot",
  "tags",
  "shield",
  "check-square",
  "activity",
  "settings",
] as const;
export type CrmModuleIconKey = (typeof crmModuleIconKeys)[number];

export type CrmModuleRegistryItem = {
  id: string;
  titleKey: CrmModuleTextKey;
  descriptionKey: CrmModuleTextKey;
  implementationPhaseKey: CrmModuleTextKey;
  path: string;
  group: CrmModuleGroup;
  iconKey: CrmModuleIconKey;
  status: CrmModuleStatus;
  backendModuleFolder: string;
  endpointDiscoveryStatus: "ready" | "source_visible" | "contract_pending" | "disabled";
};

function moduleTextKeys(
  id: string,
): Pick<CrmModuleRegistryItem, "titleKey" | "descriptionKey" | "implementationPhaseKey"> {
  return {
    titleKey: `crm.modules.${id}.title`,
    descriptionKey: `crm.modules.${id}.description`,
    implementationPhaseKey: `crm.modules.${id}.implementationPhase`,
  };
}

export const crmModuleRegistry: CrmModuleRegistryItem[] = [
  {
    id: "dashboard",
    ...moduleTextKeys("dashboard"),
    path: "/dashboard",
    group: "core",
    iconKey: "layout-dashboard",
    status: "active",
    backendModuleFolder: "AnalyticsReporting",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "customers",
    ...moduleTextKeys("customers"),
    path: "/customers",
    group: "core",
    iconKey: "users",
    status: "active",
    backendModuleFolder: "CustomerManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "companies",
    ...moduleTextKeys("companies"),
    path: "/companies",
    group: "core",
    iconKey: "building-2",
    status: "active",
    backendModuleFolder: "CustomerManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "contacts",
    ...moduleTextKeys("contacts"),
    path: "/contacts",
    group: "core",
    iconKey: "contact",
    status: "active",
    backendModuleFolder: "CustomerManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "customer-intelligence",
    ...moduleTextKeys("customer-intelligence"),
    path: "/customer-intelligence",
    group: "core",
    iconKey: "sparkles",
    status: "active",
    backendModuleFolder: "CustomerIntelligence",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "lead-management",
    ...moduleTextKeys("lead-management"),
    path: "/leads",
    group: "sales",
    iconKey: "user-plus",
    status: "active",
    backendModuleFolder: "LeadManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "deal-management",
    ...moduleTextKeys("deal-management"),
    path: "/deals",
    group: "sales",
    iconKey: "handshake",
    status: "active",
    backendModuleFolder: "DealManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "opportunity-management",
    ...moduleTextKeys("opportunity-management"),
    path: "/opportunities",
    group: "sales",
    iconKey: "target",
    status: "active",
    backendModuleFolder: "OpportunityManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "pipeline-management",
    ...moduleTextKeys("pipeline-management"),
    path: "/pipeline",
    group: "sales",
    iconKey: "git-branch",
    status: "active",
    backendModuleFolder: "PipelineManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "quote-management",
    ...moduleTextKeys("quote-management"),
    path: "/quotes",
    group: "sales",
    iconKey: "file-text",
    status: "active",
    backendModuleFolder: "QuoteManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "sales-forecasting",
    ...moduleTextKeys("sales-forecasting"),
    path: "/sales-forecasting",
    group: "sales",
    iconKey: "trending-up",
    status: "active",
    backendModuleFolder: "SalesForecasting",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "product-catalog",
    ...moduleTextKeys("product-catalog"),
    path: "/product-catalog",
    group: "sales",
    iconKey: "package",
    status: "active",
    backendModuleFolder: "ProductCatalog",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "support-inbox",
    ...moduleTextKeys("support-inbox"),
    path: "/support-inbox",
    group: "service_support",
    iconKey: "inbox",
    status: "active",
    backendModuleFolder: "SupportInboxIntegration",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "ticket-management",
    ...moduleTextKeys("ticket-management"),
    path: "/tickets",
    group: "service_support",
    iconKey: "ticket",
    status: "active",
    backendModuleFolder: "TicketManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "ticket-sla",
    ...moduleTextKeys("ticket-sla"),
    path: "/ticket-sla",
    group: "service_support",
    iconKey: "alarm-clock",
    status: "active",
    backendModuleFolder: "TicketSlaManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "ticket-workflow",
    ...moduleTextKeys("ticket-workflow"),
    path: "/ticket-workflows",
    group: "service_support",
    iconKey: "shuffle",
    status: "active",
    backendModuleFolder: "TicketWorkflowManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "marketing-automation",
    ...moduleTextKeys("marketing-automation"),
    path: "/marketing",
    group: "marketing",
    iconKey: "megaphone",
    status: "active",
    backendModuleFolder: "MarketingAutomation",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "omnichannel",
    ...moduleTextKeys("omnichannel"),
    path: "/omnichannel",
    group: "marketing",
    iconKey: "messages-square",
    status: "active",
    backendModuleFolder: "Omnichannel",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "calendar-sync",
    ...moduleTextKeys("calendar-sync"),
    path: "/calendar-sync",
    group: "operations",
    iconKey: "calendar",
    status: "active",
    backendModuleFolder: "CalendarSync",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "contract-lifecycle",
    ...moduleTextKeys("contract-lifecycle"),
    path: "/contracts",
    group: "operations",
    iconKey: "file-signature",
    status: "active",
    backendModuleFolder: "ContractLifecycle",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "document-management",
    ...moduleTextKeys("document-management"),
    path: "/documents",
    group: "operations",
    iconKey: "folder-open",
    status: "active",
    backendModuleFolder: "DocumentManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "finance-operations",
    ...moduleTextKeys("finance-operations"),
    path: "/finance",
    group: "operations",
    iconKey: "wallet",
    status: "active",
    backendModuleFolder: "FinanceOperations",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "integration-hub",
    ...moduleTextKeys("integration-hub"),
    path: "/integrations",
    group: "operations",
    iconKey: "plug",
    status: "active",
    backendModuleFolder: "IntegrationHub",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "knowledge-base",
    ...moduleTextKeys("knowledge-base"),
    path: "/knowledge-base",
    group: "operations",
    iconKey: "book-open",
    status: "active",
    backendModuleFolder: "KnowledgeBaseManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "work-management",
    ...moduleTextKeys("work-management"),
    path: "/work-management",
    group: "operations",
    iconKey: "list-checks",
    status: "active",
    backendModuleFolder: "WorkManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "workflow-automation",
    ...moduleTextKeys("workflow-automation"),
    path: "/workflows",
    group: "operations",
    iconKey: "workflow",
    status: "active",
    backendModuleFolder: "WorkflowAutomation",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "analytics-reporting",
    ...moduleTextKeys("analytics-reporting"),
    path: "/analytics",
    group: "intelligence_ai",
    iconKey: "bar-chart-3",
    status: "active",
    backendModuleFolder: "AnalyticsReporting",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "artificial-intelligence",
    ...moduleTextKeys("artificial-intelligence"),
    path: "/ai",
    group: "intelligence_ai",
    iconKey: "bot",
    status: "active",
    backendModuleFolder: "ArtificialIntelligence",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "tag-management",
    ...moduleTextKeys("tag-management"),
    path: "/tags",
    group: "administration",
    iconKey: "tags",
    status: "active",
    backendModuleFolder: "TagManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "tenant-management",
    ...moduleTextKeys("tenant-management"),
    path: "/tenants",
    group: "administration",
    iconKey: "shield",
    status: "active",
    backendModuleFolder: "TenantManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "tasks",
    ...moduleTextKeys("tasks"),
    path: "/tasks",
    group: "operations",
    iconKey: "check-square",
    status: "active",
    backendModuleFolder: "WorkManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "activities",
    ...moduleTextKeys("activities"),
    path: "/activities",
    group: "operations",
    iconKey: "activity",
    status: "active",
    backendModuleFolder: "WorkManagement",
    endpointDiscoveryStatus: "ready",
  },
  {
    id: "settings",
    ...moduleTextKeys("settings"),
    path: "/settings",
    group: "administration",
    iconKey: "settings",
    status: "coming_soon",
    backendModuleFolder: "TenantManagement",
    endpointDiscoveryStatus: "disabled",
  },
  {
    id: "trash",
    ...moduleTextKeys("trash"),
    path: "/trash",
    group: "administration",
    iconKey: "folder-open",
    status: "active",
    backendModuleFolder: "CustomerManagement",
    endpointDiscoveryStatus: "ready",
  },
];

export const crmModuleGroups: CrmModuleGroup[] = [
  "core",
  "sales",
  "service_support",
  "marketing",
  "operations",
  "intelligence_ai",
  "administration",
];

export function getCrmModuleByPath(path: string): CrmModuleRegistryItem | undefined {
  return crmModuleRegistry.find((moduleItem) => moduleItem.path === path);
}

export function getCrmModuleById(id: string): CrmModuleRegistryItem | undefined {
  return crmModuleRegistry.find((moduleItem) => moduleItem.id === id);
}

export function getCrmModulesByGroup(group: CrmModuleGroup): CrmModuleRegistryItem[] {
  return crmModuleRegistry.filter((moduleItem) => moduleItem.group === group);
}

export function isCrmModuleNavigable(moduleItem: CrmModuleRegistryItem): boolean {
  return (
    moduleItem.endpointDiscoveryStatus === "ready" &&
    (moduleItem.status === "active" || moduleItem.status === "read_only")
  );
}

export function getCrmModuleRequiredCapability(
  moduleItem: CrmModuleRegistryItem,
): CrmCapability | null {
  switch (moduleItem.id) {
    case "customers":
      return "customers.read";
    case "companies":
      return "companies.read";
    case "contacts":
      return "contacts.read";
    case "customer-intelligence":
      return "customerIntelligence.read";
    case "lead-management":
      return "leads.read";
    case "deal-management":
      return "deals.read";
    case "opportunity-management":
      return "opportunities.read";
    case "quote-management":
      return "quotes.read";
    case "product-catalog":
      return "productCatalog.read";
    case "support-inbox":
      return "supportInbox.read";
    case "ticket-management":
      return "tickets.read";
    case "ticket-sla":
      return "ticketSla.read";
    case "ticket-workflow":
      return "ticketWorkflow.read";
    case "pipeline-management":
      return "pipeline.read";
    case "sales-forecasting":
      return "salesForecasting.read";
    case "marketing-automation":
      return "marketing.read";
    case "omnichannel":
      return "omnichannel.read";
    case "calendar-sync":
      return "calendarSync.read";
    case "contract-lifecycle":
      return "contracts.read";
    case "document-management":
      return "documents.read";
    case "finance-operations":
      return "finance.read";
    case "integration-hub":
      return "integrations.read";
    case "knowledge-base":
      return "knowledgeBase.read";
    case "workflow-automation":
      return "workflows.read";
    case "analytics-reporting":
      return "analytics.read";
    case "artificial-intelligence":
      return "ai.read";
    case "tag-management":
      return "tags.read";
    case "tenant-management":
      return "tenants.read";
    case "activities":
      return "activities.read";
    case "trash":
      return "contacts.read";
    case "work-management":
    case "tasks":
      return "tasks.read";
    default:
      return null;
  }
}

export function canNavigateCrmModule(
  moduleItem: CrmModuleRegistryItem,
  capabilities: CrmCapabilities | null | undefined,
): boolean {
  return (
    isCrmModuleNavigable(moduleItem) &&
    crmCapabilityAllows(capabilities, getCrmModuleRequiredCapability(moduleItem))
  );
}
