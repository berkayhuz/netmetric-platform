import { describe, expect, it } from "vitest";

describe("Auth session-status mapping", () => {
  it("treats a valid API payload as authenticated session", async () => {
    process.env.NEXT_PUBLIC_APP_ORIGIN = "http://localhost:7002";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "http://localhost:5030";
    const { mapSessionStatusToAuthSessionStatus } = await import("./auth-api");

    const session = mapSessionStatusToAuthSessionStatus({
      tenantId: "tenant-1",
      userId: "user-1",
      email: "dev@netmetric.local",
    });

    expect(session.authenticated).toBe(true);
    expect(session.activeTenantId).toBe("tenant-1");
    expect(session.user?.id).toBe("user-1");
  });

  it("treats invalid payloads as unauthenticated", async () => {
    process.env.NEXT_PUBLIC_APP_ORIGIN = "http://localhost:7002";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "http://localhost:5030";
    const { mapSessionStatusToAuthSessionStatus } = await import("./auth-api");

    const session = mapSessionStatusToAuthSessionStatus({
      tenantId: "tenant-1",
      userId: 42,
      email: "dev@netmetric.local",
    });

    expect(session).toEqual({ authenticated: false });
  });
});
