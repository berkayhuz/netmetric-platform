import { isWarmupSafeHref } from "@netmetric/observability/performance";
import { describe, expect, it } from "vitest";

import { crmCriticalRoutes } from "./crm-critical-routes";

describe("CRM critical route warm-up config", () => {
  it("warms only high-value authenticated CRM routes", () => {
    expect(crmCriticalRoutes.map((route) => route.href)).toEqual([
      "/dashboard",
      "/customers",
      "/product-catalog",
      "/settings",
    ]);
  });

  it("does not include logout, API, or side-effect routes", () => {
    expect(crmCriticalRoutes.every((route) => isWarmupSafeHref(route.href))).toBe(true);
    expect(crmCriticalRoutes.map((route) => route.href).join(" ")).not.toMatch(
      /logout|\/api|revoke|delete|reset/i,
    );
  });
});
