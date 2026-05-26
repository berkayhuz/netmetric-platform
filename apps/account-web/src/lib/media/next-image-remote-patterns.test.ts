import { describe, expect, it } from "vitest";

import { getNetMetricImageRemotePatterns } from "../../../../next-image-remote-patterns";

describe("next image remote patterns", () => {
  it("includes local media origins and production CDN", () => {
    const patterns = getNetMetricImageRemotePatterns();

    expect(
      patterns.some(
        (pattern) =>
          pattern.protocol === "http" &&
          pattern.hostname === "localhost" &&
          pattern.port === "5301" &&
          pattern.pathname === "/uploads/**",
      ),
    ).toBe(true);

    expect(
      patterns.some(
        (pattern) =>
          pattern.protocol === "http" &&
          pattern.hostname === "127.0.0.1" &&
          pattern.port === "5301" &&
          pattern.pathname === "/uploads/**",
      ),
    ).toBe(true);

    expect(
      patterns.some(
        (pattern) =>
          pattern.protocol === "https" &&
          pattern.hostname === "cdn.netmetric.net" &&
          pattern.pathname === "/**",
      ),
    ).toBe(true);
  });
});
