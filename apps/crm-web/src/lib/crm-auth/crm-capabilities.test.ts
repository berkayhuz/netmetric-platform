import { describe, expect, it } from "vitest";

import {
  createCrmCapabilities,
  crmCapabilityAllows,
  getRequiredCrmCapabilityForPath,
} from "./crm-capabilities";

describe("CRM capability mapping", () => {
  it("denies unknown or missing permissions by default", () => {
    const capabilities = createCrmCapabilities(["unknown.permission"]);

    expect(crmCapabilityAllows(capabilities, "customers.create")).toBe(false);
    expect(crmCapabilityAllows(capabilities, "customers.read")).toBe(false);
  });

  it("maps backend permissions to frontend capabilities", () => {
    const capabilities = createCrmCapabilities([
      "crm.customer-management.customers.read",
      "customers.write",
    ]);

    expect(crmCapabilityAllows(capabilities, "customers.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "customers.create")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "customers.delete")).toBe(false);
  });

  it("maps explicit backend permissions to named CRM action capabilities", () => {
    const capabilities = createCrmCapabilities([
      "leads.manage",
      "deals.manage",
      "customers.export",
      "customers.import",
      "customer-intelligence.duplicates.read",
      "opportunities.delete",
      "win-loss.manage",
    ]);

    expect(crmCapabilityAllows(capabilities, "canCreateLead")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "canEditDeal")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "canExportCustomer")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "canImportCustomer")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "customers.duplicates.review")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "canDeleteOpportunity")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "winLoss.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "winLoss.manage")).toBe(true);
  });

  it("maps service permissions to service module capabilities", () => {
    const capabilities = createCrmCapabilities([
      "support-inbox.messages.read",
      "ticket.sla-policies.manage",
      "ticket.queues.read",
      "catalog.products.manage",
      "customer-intelligence.health.read",
      "work-management.manage",
      "contracts.manage",
      "finance.operations.manage",
      "tags.manage",
    ]);

    expect(crmCapabilityAllows(capabilities, "supportInbox.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "ticketSla.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "ticketSla.manage")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "ticketWorkflow.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "ticketWorkflow.manage")).toBe(false);
    expect(crmCapabilityAllows(capabilities, "productCatalog.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "productCatalog.manage")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "customerIntelligence.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tasks.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tasks.create")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tasks.edit")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tasks.delete")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tasks.manage")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "activities.read")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "activities.create")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "contracts.manage")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "finance.manage")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tags.manage")).toBe(true);
  });

  it("allows known capabilities for wildcard tenant owners", () => {
    const capabilities = createCrmCapabilities(["*"]);

    expect(crmCapabilityAllows(capabilities, "customers.create")).toBe(true);
    expect(crmCapabilityAllows(capabilities, "tickets.delete")).toBe(true);
  });

  it("maps CRM entity routes to the required capability", () => {
    expect(getRequiredCrmCapabilityForPath("/customers")).toBe("customers.read");
    expect(getRequiredCrmCapabilityForPath("/customers/new")).toBe("customers.create");
    expect(
      getRequiredCrmCapabilityForPath("/customers/6f9619ff-8b86-d011-b42d-00cf4fc964ff/edit"),
    ).toBe("customers.edit");
    expect(getRequiredCrmCapabilityForPath("/quotes/6f9619ff-8b86-d011-b42d-00cf4fc964ff")).toBe(
      "quotes.read",
    );
    expect(getRequiredCrmCapabilityForPath("/tasks/new")).toBe("tasks.create");
    expect(getRequiredCrmCapabilityForPath("/tasks/meetings/new")).toBe("tasks.create");
    expect(getRequiredCrmCapabilityForPath("/tasks/123/edit")).toBe("tasks.edit");
    expect(getRequiredCrmCapabilityForPath("/support-inbox")).toBe("supportInbox.read");
    expect(getRequiredCrmCapabilityForPath("/ticket-sla")).toBe("ticketSla.read");
    expect(getRequiredCrmCapabilityForPath("/ticket-workflows")).toBe("ticketWorkflow.read");
    expect(getRequiredCrmCapabilityForPath("/customer-intelligence")).toBe(
      "customerIntelligence.read",
    );
    expect(getRequiredCrmCapabilityForPath("/product-catalog")).toBe("productCatalog.read");
    expect(getRequiredCrmCapabilityForPath("/product-catalog/new")).toBe("productCatalog.manage");
    expect(getRequiredCrmCapabilityForPath("/product-catalog/abc/edit")).toBe(
      "productCatalog.manage",
    );
    expect(getRequiredCrmCapabilityForPath("/product-catalog/categories")).toBe(
      "productCatalog.read",
    );
    expect(getRequiredCrmCapabilityForPath("/product-catalog/categories/new")).toBe(
      "productCatalog.manage",
    );
    expect(getRequiredCrmCapabilityForPath("/product-catalog/categories/abc/edit")).toBe(
      "productCatalog.manage",
    );
    expect(getRequiredCrmCapabilityForPath("/activities")).toBe("activities.read");
    expect(getRequiredCrmCapabilityForPath("/unknown")).toBeNull();
  });
});
