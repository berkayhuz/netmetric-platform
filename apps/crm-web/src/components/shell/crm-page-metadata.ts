export type CrmPageMetadata = {
  titleKey: string;
  descriptionKey: string;
};

export const crmPageMetadata = {
  "/dashboard": {
    titleKey: "crm.dashboard.title",
    descriptionKey: "crm.dashboard.description",
  },
  "/customers": {
    titleKey: "crm.customers.pages.list.title",
    descriptionKey: "crm.customers.pages.list.description",
  },
  "/customers/new": {
    titleKey: "crm.customers.pages.create.title",
    descriptionKey: "crm.customers.pages.create.description",
  },
  "/customers/[id]/edit": {
    titleKey: "crm.customers.pages.edit.title",
    descriptionKey: "crm.customers.pages.edit.description",
  },
  "/companies": {
    titleKey: "crm.companies.pages.list.title",
    descriptionKey: "crm.companies.pages.list.description",
  },
  "/companies/new": {
    titleKey: "crm.companies.pages.create.title",
    descriptionKey: "crm.companies.pages.create.description",
  },
  "/companies/[id]/edit": {
    titleKey: "crm.companies.pages.edit.title",
    descriptionKey: "crm.companies.pages.edit.description",
  },
  "/contacts": {
    titleKey: "crm.contacts.pages.list.title",
    descriptionKey: "crm.contacts.pages.list.description",
  },
  "/contacts/new": {
    titleKey: "crm.contacts.pages.create.title",
    descriptionKey: "crm.contacts.pages.create.description",
  },
  "/contacts/[id]/edit": {
    titleKey: "crm.contacts.pages.edit.title",
    descriptionKey: "crm.contacts.pages.edit.description",
  },
  "/leads": {
    titleKey: "crm.leads.pages.list.title",
    descriptionKey: "crm.leads.pages.list.description",
  },
  "/leads/new": {
    titleKey: "crm.leads.pages.create.title",
    descriptionKey: "crm.leads.pages.create.description",
  },
  "/leads/[id]/edit": {
    titleKey: "crm.leads.pages.edit.title",
    descriptionKey: "crm.leads.pages.edit.description",
  },
  "/deals": {
    titleKey: "crm.deals.pages.list.title",
    descriptionKey: "crm.deals.pages.list.description",
  },
  "/deals/new": {
    titleKey: "crm.deals.pages.create.title",
    descriptionKey: "crm.deals.pages.create.description",
  },
  "/deals/[id]/edit": {
    titleKey: "crm.deals.pages.edit.title",
    descriptionKey: "crm.deals.pages.edit.description",
  },
  "/opportunities": {
    titleKey: "crm.opportunities.pages.list.title",
    descriptionKey: "crm.opportunities.pages.list.description",
  },
  "/opportunities/new": {
    titleKey: "crm.opportunities.pages.create.title",
    descriptionKey: "crm.opportunities.pages.create.description",
  },
  "/opportunities/[id]/edit": {
    titleKey: "crm.opportunities.pages.edit.title",
    descriptionKey: "crm.opportunities.pages.edit.description",
  },
  "/quotes": {
    titleKey: "crm.quotes.title",
    descriptionKey: "crm.quotes.description",
  },
  "/quotes/new": {
    titleKey: "crm.quotes.create.title",
    descriptionKey: "crm.quotes.create.description",
  },
  "/quotes/[id]/edit": {
    titleKey: "crm.quotes.edit.title",
    descriptionKey: "crm.quotes.edit.description",
  },
  "/tickets": {
    titleKey: "crm.tickets.title",
    descriptionKey: "crm.tickets.description",
  },
  "/tickets/new": {
    titleKey: "crm.tickets.new.title",
    descriptionKey: "crm.tickets.new.description",
  },
  "/tickets/[id]/edit": {
    titleKey: "crm.tickets.edit.title",
    descriptionKey: "crm.tickets.edit.description",
  },
  "/tasks": {
    titleKey: "crm.tasks.page.title",
    descriptionKey: "crm.tasks.page.description",
  },
  "/tasks/new": {
    titleKey: "crm.tasks.new.title",
    descriptionKey: "crm.tasks.new.description",
  },
  "/tasks/meetings/new": {
    titleKey: "crm.meetings.new.title",
    descriptionKey: "crm.meetings.new.description",
  },
  "/pipeline": {
    titleKey: "crm.pipeline.title",
    descriptionKey: "crm.pipeline.description",
  },
  "/support-inbox": {
    titleKey: "crm.supportInbox.page.title",
    descriptionKey: "crm.supportInbox.page.description",
  },
  "/ticket-sla": {
    titleKey: "crm.ticketSla.page.title",
    descriptionKey: "crm.ticketSla.page.description",
  },
  "/ticket-workflows": {
    titleKey: "crm.ticketWorkflows.page.title",
    descriptionKey: "crm.ticketWorkflows.page.description",
  },
  "/product-catalog": {
    titleKey: "crm.productCatalog.pages.list.title",
    descriptionKey: "crm.productCatalog.pages.list.description",
  },
  "/product-catalog/new": {
    titleKey: "crm.productCatalog.pages.create.title",
    descriptionKey: "crm.productCatalog.pages.create.description",
  },
  "/product-catalog/[id]/edit": {
    titleKey: "crm.productCatalog.pages.edit.title",
    descriptionKey: "crm.productCatalog.pages.edit.description",
  },
  "/product-catalog/categories": {
    titleKey: "crm.productCatalog.categories.pages.list.title",
    descriptionKey: "crm.productCatalog.categories.pages.list.description",
  },
  "/product-catalog/categories/new": {
    titleKey: "crm.productCatalog.categories.pages.create.title",
    descriptionKey: "crm.productCatalog.categories.pages.create.description",
  },
  "/product-catalog/categories/[id]/edit": {
    titleKey: "crm.productCatalog.categories.pages.edit.title",
    descriptionKey: "crm.productCatalog.categories.pages.edit.description",
  },
} as const satisfies Record<string, CrmPageMetadata>;

export type CrmPagePath = keyof typeof crmPageMetadata;

export function getCrmPageMetadata(path: CrmPagePath): CrmPageMetadata {
  return crmPageMetadata[path];
}
