import { describe, expect, it } from "vitest";

import { createCrmCapabilities } from "@/lib/crm-auth/crm-capabilities";

import { canNavigateCrmModule, getCrmModuleById, isCrmModuleNavigable } from "./module-registry";

describe("CRM module registry", () => {
  it("opens customer intelligence when the API-backed workspace capability is present", () => {
    const moduleItem = getCrmModuleById("customer-intelligence");

    expect(moduleItem).toBeDefined();
    expect(moduleItem?.status).toBe("active");
    expect(isCrmModuleNavigable(moduleItem!)).toBe(true);
    expect(
      canNavigateCrmModule(
        moduleItem!,
        createCrmCapabilities(["customer-intelligence.duplicates.read"]),
      ),
    ).toBe(true);
  });

  it("keeps ready modules navigable when capabilities allow them", () => {
    const moduleItem = getCrmModuleById("customers");

    expect(moduleItem).toBeDefined();
    expect(isCrmModuleNavigable(moduleItem!)).toBe(true);
    expect(moduleItem?.endpointDiscoveryStatus).toBe("ready");
    expect(canNavigateCrmModule(moduleItem!, createCrmCapabilities(["customers.read"]))).toBe(true);
  });

  it("navigates product catalog when catalog permissions are granted", () => {
    const moduleItem = getCrmModuleById("product-catalog");

    expect(moduleItem).toBeDefined();
    expect(moduleItem?.status).toBe("active");
    expect(moduleItem?.endpointDiscoveryStatus).toBe("ready");
    expect(
      canNavigateCrmModule(moduleItem!, createCrmCapabilities(["catalog.products.read"])),
    ).toBe(true);
  });

  it("keeps settings out of navigation until an API contract exists", () => {
    const moduleItem = getCrmModuleById("settings");

    expect(moduleItem).toBeDefined();
    expect(moduleItem?.endpointDiscoveryStatus).toBe("disabled");
    expect(isCrmModuleNavigable(moduleItem!)).toBe(false);
  });

  it("denies ready modules when the capability map does not allow them", () => {
    const moduleItem = getCrmModuleById("customers");

    expect(moduleItem).toBeDefined();
    expect(canNavigateCrmModule(moduleItem!, createCrmCapabilities(["profile:self"]))).toBe(false);
  });
});
