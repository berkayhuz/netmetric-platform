import { afterEach, describe, expect, it, vi } from "vitest";

import { GET as getLive } from "@/app/health/live/route";
import { GET as getReady } from "@/app/health/ready/route";

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("public-web health routes", () => {
  it("returns live ok", async () => {
    const response = await getLive();
    const json = await response.json();
    expect(response.status).toBe(200);
    expect(json).toEqual({ status: "ok" });
  });

  it("returns ready", async () => {
    vi.stubEnv("NEXT_PUBLIC_SITE_URL", "https://netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_AUTH_URL", "https://auth.netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_ACCOUNT_URL", "https://account.netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_CRM_URL", "https://crm.netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_TOOLS_URL", "https://tools.netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_API_URL", "https://api.netmetric.net");

    const response = await getReady();
    const json = await response.json();
    expect(response.status).toBe(200);
    expect(json).toEqual({ status: "ready" });
  });
});
