import { isWarmupSafeHref } from "@netmetric/observability/performance";
import { describe, expect, it } from "vitest";

import { accountCriticalRoutes } from "./account-critical-routes";

describe("account critical route warm-up config", () => {
  it("warms only the authenticated account routes that matter after login", () => {
    expect(accountCriticalRoutes.map((route) => route.href)).toEqual([
      "/profile",
      "/preferences",
      "/security",
      "/settings/team",
    ]);
  });

  it("does not include logout, API, or side-effect routes", () => {
    expect(accountCriticalRoutes.every((route) => isWarmupSafeHref(route.href))).toBe(true);
    expect(accountCriticalRoutes.map((route) => route.href).join(" ")).not.toMatch(
      /logout|\/api|revoke|delete|reset/i,
    );
  });
});
