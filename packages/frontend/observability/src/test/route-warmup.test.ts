import { describe, expect, it } from "vitest";

import {
  createCriticalRouteWarmupConfig,
  getWarmupHrefs,
  isWarmupSafeHref,
} from "../performance/route-warmup-core";

describe("critical route warm-up safety", () => {
  it("allows only same-origin navigational routes", () => {
    expect(isWarmupSafeHref("/dashboard")).toBe(true);
    expect(isWarmupSafeHref("/customers?page=1")).toBe(true);

    expect(isWarmupSafeHref("https://example.com/dashboard")).toBe(false);
    expect(isWarmupSafeHref("//example.com/dashboard")).toBe(false);
    expect(isWarmupSafeHref("/api/auth/logout")).toBe(false);
    expect(isWarmupSafeHref("/security/sessions/revoke")).toBe(false);
    expect(isWarmupSafeHref("/auth/session-reset")).toBe(false);
  });

  it("rejects unsafe configured routes early", () => {
    expect(() =>
      createCriticalRouteWarmupConfig([
        { href: "/profile", label: "Profile" },
        { href: "/api/auth/logout", label: "Logout" },
      ] as const),
    ).toThrow(/Unsafe critical route warm-up href/);
  });

  it("dedupes, skips the current page, and respects the max route cap", () => {
    const routes = createCriticalRouteWarmupConfig([
      { href: "/profile", label: "Profile" },
      { href: "/security", label: "Security" },
      { href: "/security", label: "Security duplicate" },
      { href: "/preferences", label: "Preferences" },
    ] as const);

    expect(getWarmupHrefs(routes, "/profile", 2)).toEqual(["/security", "/preferences"]);
  });
});
