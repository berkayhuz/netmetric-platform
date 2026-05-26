import { describe, expect, it } from "vitest";

import { resolveLocaleCookieOptions } from "./locale-cookie";

describe("resolveLocaleCookieOptions", () => {
  it("uses shared domain in production-like environments", () => {
    const result = resolveLocaleCookieOptions({
      appOrigin: "https://account.netmetric.net",
      cookieDomain: ".netmetric.net",
    });

    expect(result.domain).toBe(".netmetric.net");
    expect(result.secure).toBe(true);
    expect(result.sameSite).toBe("lax");
    expect(result.path).toBe("/");
  });

  it("omits domain for localhost and keeps local cookie behavior", () => {
    const result = resolveLocaleCookieOptions({
      appOrigin: "http://localhost:7004",
      cookieDomain: ".netmetric.net",
    });

    expect(result.domain).toBeUndefined();
    expect(result.secure).toBe(false);
    expect(result.sameSite).toBe("lax");
    expect(result.path).toBe("/");
  });

  it("omits domain for localhost-like configured domains", () => {
    const result = resolveLocaleCookieOptions({
      appOrigin: "https://account.netmetric.net",
      cookieDomain: "localhost",
    });

    expect(result.domain).toBeUndefined();
    expect(result.secure).toBe(true);
  });
});
