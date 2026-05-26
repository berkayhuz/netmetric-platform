import { describe, expect, it } from "vitest";

import { publicSiteConfig } from "../site";

describe("site config contract", () => {
  it("exposes required site metadata", () => {
    expect(publicSiteConfig.name).toBe("NetMetric");
    expect(publicSiteConfig.url).toMatch(/^https?:\/\//);
    expect(publicSiteConfig.nav.length).toBeGreaterThan(0);
  });
});
