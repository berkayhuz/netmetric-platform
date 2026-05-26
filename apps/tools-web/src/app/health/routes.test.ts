import { afterEach, describe, expect, it, vi } from "vitest";

import { GET as getLive } from "@/app/health/live/route";
import { GET as getReady } from "@/app/health/ready/route";

afterEach(() => {
  vi.unstubAllEnvs();
});

describe("tools-web health routes", () => {
  it("returns live ok", async () => {
    const response = getLive();
    const json = await response.json();
    expect(response.status).toBe(200);
    expect(json).toEqual({ status: "ok" });
  });

  it("returns ready", async () => {
    vi.stubEnv("NEXT_PUBLIC_AUTH_URL", "https://auth.netmetric.net");
    vi.stubEnv("NEXT_PUBLIC_API_BASE_URL", "https://api.netmetric.net");

    const response = getReady();
    const json = await response.json();
    expect(response.status).toBe(200);
    expect(json).toEqual({ status: "ready" });
  });
});
