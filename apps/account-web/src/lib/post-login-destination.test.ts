import { beforeEach, describe, expect, it, vi } from "vitest";

describe("post login destination", () => {
  beforeEach(() => {
    vi.resetModules();
    process.env.NEXT_PUBLIC_ACCOUNT_URL = "http://localhost:7004";
    process.env.NEXT_PUBLIC_TOOLS_URL = "http://localhost:7005";
    process.env.NEXT_PUBLIC_CRM_URL = "http://localhost:7006";
    process.env.NEXT_PUBLIC_PUBLIC_URL = "http://localhost:7001";
  });

  it("maps enum-like keys to configured URLs", async () => {
    const { resolvePostLoginDestinationUrl } = await import("./post-login-destination");

    expect(resolvePostLoginDestinationUrl("Account")).toBe("http://localhost:7004/");
    expect(resolvePostLoginDestinationUrl("Tools")).toBe("http://localhost:7005/");
    expect(resolvePostLoginDestinationUrl("Crm")).toBe("http://localhost:7006/");
    expect(resolvePostLoginDestinationUrl("Public")).toBe("http://localhost:7001/");
  });

  it("falls back safely for invalid values", async () => {
    const { resolvePostLoginDestination, resolvePostLoginDestinationUrl } =
      await import("./post-login-destination");

    expect(resolvePostLoginDestination("https://evil.example")).toBe("Account");
    expect(resolvePostLoginDestinationUrl("https://evil.example")).toBe("http://localhost:7004/");
  });
});
