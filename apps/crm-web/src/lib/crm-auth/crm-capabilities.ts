export const crmCapabilityNames = [
  "customers.read",
  "customers.create",
  "customers.edit",
  "customers.delete",
  "customers.duplicates.review",
  "companies.read",
  "companies.create",
  "companies.edit",
  "companies.delete",
  "contacts.read",
  "contacts.create",
  "contacts.edit",
  "contacts.delete",
  "leads.read",
  "leads.create",
  "leads.edit",
  "leads.delete",
  "canCreateLead",
  "deals.read",
  "deals.create",
  "deals.edit",
  "deals.delete",
  "canEditDeal",
  "winLoss.read",
  "winLoss.manage",
  "opportunities.read",
  "opportunities.create",
  "opportunities.edit",
  "opportunities.delete",
  "canDeleteOpportunity",
  "opportunityQuotes.read",
  "opportunityQuotes.manage",
  "quotes.read",
  "quotes.create",
  "quotes.edit",
  "quotes.delete",
  "proposals.read",
  "proposals.manage",
  "productCatalog.read",
  "productCatalog.manage",
  "tickets.read",
  "tickets.create",
  "tickets.edit",
  "tickets.delete",
  "supportInbox.read",
  "supportInbox.manage",
  "ticketSla.read",
  "ticketSla.manage",
  "ticketWorkflow.read",
  "ticketWorkflow.manage",
  "pipeline.read",
  "pipeline.manage",
  "pipelineStageHistory.read",
  "pipelineLostReasons.read",
  "pipelineLostReasons.manage",
  "tasks.read",
  "tasks.create",
  "tasks.edit",
  "tasks.delete",
  "tasks.manage",
  "customerIntelligence.read",
  "salesForecasting.read",
  "marketing.read",
  "omnichannel.read",
  "calendarSync.read",
  "contracts.read",
  "contracts.manage",
  "documents.read",
  "finance.read",
  "finance.manage",
  "integrations.read",
  "knowledgeBase.read",
  "workflows.read",
  "analytics.read",
  "ai.read",
  "tags.read",
  "tags.manage",
  "tenants.read",
  "activities.read",
  "activities.create",
  "canExportCustomer",
  "canImportCustomer",
] as const;

export type CrmCapability = (typeof crmCapabilityNames)[number];
export type CrmCapabilities = Record<CrmCapability, boolean>;

const permissionAliases: Record<CrmCapability, readonly string[]> = {
  "customers.read": [
    "customers.read",
    "crm.customer-management.customers.read",
    "crm.customer-management.customers.manage",
  ],
  "customers.create": [
    "customers.write",
    "customers.manage",
    "crm.customer-management.customers.manage",
  ],
  "customers.edit": [
    "customers.write",
    "customers.manage",
    "crm.customer-management.customers.manage",
  ],
  "customers.delete": [
    "customers.delete",
    "customers.manage",
    "crm.customer-management.customers.manage",
  ],
  "customers.duplicates.review": [
    "customer-intelligence.duplicates.read",
    "customer-intelligence.duplicates.manage",
    "customers.manage",
    "crm.customer-management.customers.manage",
  ],
  "companies.read": [
    "companies.read",
    "crm.customer-management.companies.read",
    "crm.customer-management.companies.manage",
  ],
  "companies.create": ["companies.manage", "crm.customer-management.companies.manage"],
  "companies.edit": ["companies.manage", "crm.customer-management.companies.manage"],
  "companies.delete": ["companies.manage", "crm.customer-management.companies.manage"],
  "contacts.read": [
    "contacts.read",
    "crm.customer-management.contacts.read",
    "crm.customer-management.contacts.manage",
  ],
  "contacts.create": ["contacts.manage", "crm.customer-management.contacts.manage"],
  "contacts.edit": ["contacts.manage", "crm.customer-management.contacts.manage"],
  "contacts.delete": ["contacts.manage", "crm.customer-management.contacts.manage"],
  "leads.read": ["leads.read"],
  "leads.create": ["leads.manage"],
  "leads.edit": ["leads.manage"],
  "leads.delete": ["leads.manage"],
  canCreateLead: ["leads.create", "leads.manage"],
  "deals.read": ["deals.read"],
  "deals.create": ["deals.manage"],
  "deals.edit": ["deals.manage"],
  "deals.delete": ["deals.manage"],
  canEditDeal: ["deals.edit", "deals.manage"],
  "winLoss.read": ["win-loss.read", "win-loss.manage"],
  "winLoss.manage": ["win-loss.manage"],
  "opportunities.read": ["opportunities.read"],
  "opportunities.create": ["opportunities.manage"],
  "opportunities.edit": ["opportunities.manage"],
  "opportunities.delete": ["opportunities.manage"],
  canDeleteOpportunity: ["opportunities.delete", "opportunities.manage"],
  "opportunityQuotes.read": ["opportunity.quotes.read", "opportunity.quotes.manage"],
  "opportunityQuotes.manage": ["opportunity.quotes.manage"],
  "quotes.read": ["quotes.read"],
  "quotes.create": ["quotes.manage"],
  "quotes.edit": ["quotes.manage"],
  "quotes.delete": ["quotes.manage"],
  "proposals.read": ["proposals.read", "proposals.manage"],
  "proposals.manage": ["proposals.manage"],
  "productCatalog.read": ["catalog.products.read", "catalog.products.manage"],
  "productCatalog.manage": ["catalog.products.manage"],
  "tickets.read": ["tickets.read"],
  "tickets.create": ["tickets.manage"],
  "tickets.edit": ["tickets.manage"],
  "tickets.delete": ["tickets.manage"],
  "supportInbox.read": [
    "support-inbox.connections.read",
    "support-inbox.rules.read",
    "support-inbox.messages.read",
    "crm.inbox.read",
  ],
  "supportInbox.manage": [
    "support-inbox.connections.manage",
    "support-inbox.rules.manage",
    "crm.inbox.manage",
  ],
  "ticketSla.read": ["ticket.sla-policies.read", "ticket.sla-policies.manage"],
  "ticketSla.manage": ["ticket.sla-policies.manage"],
  "ticketWorkflow.read": [
    "ticket.queues.read",
    "ticket.queues.manage",
    "ticket.assignments.read",
    "ticket.status-history.read",
  ],
  "ticketWorkflow.manage": ["ticket.queues.manage"],
  "pipeline.read": ["pipeline.pipelines.read", "pipeline.pipelines.manage"],
  "pipeline.manage": ["pipeline.pipelines.manage"],
  "pipelineStageHistory.read": ["pipeline.stage-history.read"],
  "pipelineLostReasons.read": ["pipeline.lost-reasons.read", "pipeline.lost-reasons.manage"],
  "pipelineLostReasons.manage": ["pipeline.lost-reasons.manage"],
  "tasks.read": [
    "tasks.read",
    "work-management.read",
    "work-management.manage",
    "work-management.tasks.read",
    "work-management.tasks.manage",
  ],
  "tasks.create": ["tasks.manage", "work-management.manage", "work-management.tasks.manage"],
  "tasks.edit": ["tasks.manage", "work-management.manage", "work-management.tasks.manage"],
  "tasks.delete": ["tasks.manage", "work-management.manage", "work-management.tasks.manage"],
  "tasks.manage": ["tasks.manage", "work-management.manage", "work-management.tasks.manage"],
  "customerIntelligence.read": [
    "customer-intelligence.duplicates.read",
    "customer-intelligence.health.read",
    "customer-intelligence.search.read",
    "customer-intelligence.timeline.read",
    "customer-intelligence.duplicates.manage",
  ],
  "salesForecasting.read": ["sales-forecasts.read", "sales-forecasts.manage"],
  "marketing.read": ["marketing.campaigns.read", "marketing.campaigns.manage"],
  "omnichannel.read": ["omnichannel.read", "omnichannel.manage"],
  "calendarSync.read": ["calendar-sync.read", "calendar-sync.manage"],
  "contracts.read": ["contracts.read", "contracts.manage"],
  "contracts.manage": ["contracts.manage"],
  "documents.read": [
    "documents.read",
    "documents.manage",
    "documents.versions.manage",
    "documents.approvals.manage",
  ],
  "finance.read": ["finance.operations.read", "finance.operations.manage", "orders.read"],
  "finance.manage": ["finance.operations.manage", "orders.manage"],
  "integrations.read": [
    "integrations.read",
    "integrations.manage",
    "crm.integrations.read",
    "crm.integrations.manage",
    "crm.apiKeys.read",
    "crm.webhooks.read",
  ],
  "knowledgeBase.read": [
    "knowledge-base.articles.read",
    "knowledge-base.articles.manage",
    "knowledge-base.categories.read",
    "knowledge-base.categories.manage",
  ],
  "workflows.read": [
    "workflow.rules.manage",
    "workflow.webhooks.manage",
    "workflow.approvals.manage",
    "workflow.assignment-rules.manage",
  ],
  "analytics.read": ["analytics.read"],
  "ai.read": ["artificial-intelligence.read", "artificial-intelligence.manage"],
  "tags.read": [
    "tags.read",
    "tags.manage",
    "tags.smart-labels.manage",
    "tags.classifications.manage",
  ],
  "tags.manage": ["tags.manage", "tags.smart-labels.manage", "tags.classifications.manage"],
  "tenants.read": ["tenants.read", "tenants.manage"],
  "activities.read": ["work-management.read", "work-management.manage", "tasks.read"],
  "activities.create": ["activities.create", "work-management.manage", "tasks.manage"],
  canExportCustomer: [
    "customers.export",
    "customers.manage",
    "crm.customer-management.customers.export",
    "crm.customer-management.customers.manage",
  ],
  canImportCustomer: [
    "customers.import",
    "customers.manage",
    "crm.customer-management.customers.import",
    "crm.customer-management.customers.manage",
  ],
};

export const emptyCrmCapabilities: CrmCapabilities = Object.fromEntries(
  crmCapabilityNames.map((capability) => [capability, false]),
) as CrmCapabilities;

export function createCrmCapabilities(permissions: readonly string[]): CrmCapabilities {
  const normalizedPermissions = new Set(
    permissions.map((permission) => permission.trim().toLowerCase()).filter(Boolean),
  );
  const allowAll = normalizedPermissions.has("*");

  return Object.fromEntries(
    crmCapabilityNames.map((capability) => [
      capability,
      allowAll ||
        permissionAliases[capability].some((permission) =>
          normalizedPermissions.has(permission.toLowerCase()),
        ),
    ]),
  ) as CrmCapabilities;
}

export function crmCapabilityAllows(
  capabilities: CrmCapabilities | null | undefined,
  capability: CrmCapability | null | undefined,
): boolean {
  if (!capability) {
    return true;
  }

  return capabilities?.[capability] === true;
}

const crmEntityPathCapabilities = {
  customers: "customers",
  companies: "companies",
  contacts: "contacts",
  leads: "leads",
  deals: "deals",
  opportunities: "opportunities",
  quotes: "quotes",
  tickets: "tickets",
} as const;

type CrmEntityPathSegment = keyof typeof crmEntityPathCapabilities;

const crmOperationalPathCapabilities: Record<string, CrmCapability> = {
  "customer-intelligence": "customerIntelligence.read",
  "sales-forecasting": "salesForecasting.read",
  marketing: "marketing.read",
  omnichannel: "omnichannel.read",
  "calendar-sync": "calendarSync.read",
  contracts: "contracts.read",
  documents: "documents.read",
  finance: "finance.read",
  integrations: "integrations.read",
  "knowledge-base": "knowledgeBase.read",
  workflows: "workflows.read",
  analytics: "analytics.read",
  ai: "ai.read",
  tags: "tags.read",
  tenants: "tenants.read",
  activities: "activities.read",
};

function normalizeCrmPath(pathname: string): string[] {
  const cleanPath = pathname.split(/[?#]/, 1)[0]?.replace(/\/+$/, "") || "/";
  return cleanPath.split("/").filter(Boolean);
}

export function getRequiredCrmCapabilityForPath(pathname: string): CrmCapability | null {
  const [firstSegment, secondSegment, thirdSegment, fourthSegment] = normalizeCrmPath(pathname);

  if (!firstSegment || firstSegment === "dashboard") {
    return null;
  }

  if (firstSegment === "tasks") {
    if ([secondSegment, thirdSegment].includes("new")) {
      return "tasks.create";
    }

    if (thirdSegment === "edit") {
      return "tasks.edit";
    }

    return "tasks.read";
  }

  if (firstSegment === "support-inbox") {
    return "supportInbox.read";
  }

  if (firstSegment === "ticket-sla") {
    return "ticketSla.read";
  }

  if (firstSegment === "ticket-workflows") {
    return "ticketWorkflow.read";
  }

  if (firstSegment === "pipeline") {
    return "pipeline.read";
  }

  if (firstSegment === "work-management") {
    return "tasks.read";
  }

  if (firstSegment === "product-catalog") {
    if (
      secondSegment === "new" ||
      thirdSegment === "edit" ||
      (secondSegment === "categories" && (thirdSegment === "new" || fourthSegment === "edit"))
    ) {
      return "productCatalog.manage";
    }

    return "productCatalog.read";
  }

  const operationalCapability = crmOperationalPathCapabilities[firstSegment];
  if (operationalCapability) {
    return operationalCapability;
  }

  const entity = crmEntityPathCapabilities[firstSegment as CrmEntityPathSegment];
  if (!entity) {
    return null;
  }

  if (secondSegment === "new") {
    return `${entity}.create` as CrmCapability;
  }

  if (thirdSegment === "edit") {
    return `${entity}.edit` as CrmCapability;
  }

  return `${entity}.read` as CrmCapability;
}
