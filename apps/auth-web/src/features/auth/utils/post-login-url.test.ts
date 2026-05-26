import { beforeEach, describe, expect, it, vi } from "vitest";

describe("post-login redirects", () => {
  beforeEach(() => {
    vi.resetModules();
    vi.unstubAllEnvs();
    Reflect.deleteProperty(globalThis, "document");
    vi.stubEnv("NODE_ENV", "development");
    delete process.env.APP_ENV;
    process.env.NEXT_PUBLIC_APP_ORIGIN = "http://localhost:7002";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "http://localhost:5030";
    process.env.NEXT_PUBLIC_ACCOUNT_URL = "http://localhost:7004";
    process.env.NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS =
      "http://localhost:7004,http://localhost:7005,http://localhost:7006";
  });

  it("defaults to account url when returnUrl is empty", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    expect(getSafePostLoginRedirectUrl()).toBe("http://localhost:7004/auth/continue");
  });

  it("keeps allowed returnUrl origin", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    expect(getSafePostLoginRedirectUrl("http://localhost:7006/dashboard?x=1")).toBe(
      "http://localhost:7006/dashboard?x=1",
    );
  });

  it("rejects unsafe external returnUrl", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    expect(getSafePostLoginRedirectUrl("https://evil.example/phish")).toBe(
      "http://localhost:7004/auth/continue",
    );
  });

  it("rejects protocol-relative returnUrl values", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    expect(getSafePostLoginRedirectUrl("//evil.example/phish")).toBe(
      "http://localhost:7004/auth/continue",
    );
  });

  it("uses post-login redirect cookie when returnUrl is missing", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    Object.defineProperty(globalThis, "document", {
      value: {
        cookie: "netmetric-post-login-redirect-url=http%3A%2F%2Flocalhost%3A7006%2Fdashboard",
      },
      configurable: true,
    });

    expect(getSafePostLoginRedirectUrl()).toBe("http://localhost:7006/dashboard");
  });

  it("rejects unsafe cookie redirect values", async () => {
    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");
    Object.defineProperty(globalThis, "document", {
      value: {
        cookie: "netmetric-post-login-redirect-url=https%3A%2F%2Fevil.example%2Fphish",
      },
      configurable: true,
    });

    expect(getSafePostLoginRedirectUrl()).toBe("http://localhost:7004/auth/continue");
  });

  it("uses the strict production origin allowlist", async () => {
    vi.resetModules();
    vi.stubEnv("NODE_ENV", "production");
    process.env.NEXT_PUBLIC_APP_ORIGIN = "https://auth.netmetric.net";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "https://api.netmetric.net";
    process.env.NEXT_PUBLIC_ACCOUNT_URL = "https://account.netmetric.net";
    process.env.NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS =
      "http://localhost:7006,https://evil.example";

    const { getSafePostLoginRedirectUrl } = await import("./post-login-url");

    expect(getSafePostLoginRedirectUrl("https://crm.netmetric.net/dashboard")).toBe(
      "https://crm.netmetric.net/dashboard",
    );
    expect(getSafePostLoginRedirectUrl("http://localhost:7006/dashboard")).toBe(
      "https://account.netmetric.net/auth/continue",
    );
    expect(getSafePostLoginRedirectUrl("https://evil.example/phish")).toBe(
      "https://account.netmetric.net/auth/continue",
    );
  });
});
