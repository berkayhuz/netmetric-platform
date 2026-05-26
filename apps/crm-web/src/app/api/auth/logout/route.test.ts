import { NextRequest } from "next/server";
import { afterEach, describe, expect, it, vi } from "vitest";

const originalFetch = global.fetch;

afterEach(() => {
  global.fetch = originalFetch;
  vi.restoreAllMocks();
});

describe("CRM logout proxy", () => {
  it("forwards cookies with an origin accepted by Auth API cookie protection", async () => {
    process.env.CRM_API_BASE_URL = "http://localhost:5030";
    process.env.CRM_AUTH_SESSION_BASE_URL = "http://localhost:5030";
    process.env.NEXT_PUBLIC_AUTH_URL = "http://localhost:7002";
    const { createLogoutRequestHeaders } = await import("./route");

    const request = new NextRequest("http://localhost:7006/api/auth/logout", {
      method: "POST",
      headers: {
        cookie:
          "__Secure-netmetric-access=access-token; __Secure-netmetric-refresh=refresh-token; __Secure-netmetric-session=session-id",
        "user-agent": "vitest",
        "x-correlation-id": "correlation-1",
      },
    });

    const headers = createLogoutRequestHeaders(request);

    expect(headers.get("cookie")).toContain("__Secure-netmetric-refresh=refresh-token");
    expect(headers.get("origin")).toBe("http://localhost:7006");
    expect(headers.get("user-agent")).toBe("vitest");
    expect(headers.get("x-request-id")).toBe("correlation-1");
  });

  it("clears auth cookies on logout response", async () => {
    process.env.CRM_API_BASE_URL = "http://localhost:5030";
    process.env.CRM_AUTH_SESSION_BASE_URL = "http://localhost:5030";
    process.env.NEXT_PUBLIC_AUTH_URL = "http://localhost:7002";
    process.env.CRM_ACCESS_COOKIE_NAME = "__Secure-netmetric-access";
    process.env.CRM_REFRESH_COOKIE_NAME = "__Secure-netmetric-refresh";
    process.env.CRM_SESSION_COOKIE_NAME = "__Secure-netmetric-session";

    global.fetch = vi.fn(async () => new Response(null, { status: 204 })) as typeof fetch;

    const { POST } = await import("./route");
    const request = new NextRequest("http://localhost:7006/api/auth/logout", {
      method: "POST",
      headers: {
        cookie:
          "__Secure-netmetric-access=access-token; __Secure-netmetric-refresh=refresh-token; __Secure-netmetric-session=session-id",
      },
    });

    const response = await POST(request);
    const payload = (await response.json()) as { redirectUrl?: string };
    const setCookie = response.headers.get("set-cookie") ?? "";

    expect(payload.redirectUrl).toBe("http://localhost:7002/login");
    expect(setCookie).toContain("__Secure-netmetric-access=");
    expect(setCookie).toContain("__Secure-netmetric-refresh=");
    expect(setCookie).toContain("__Secure-netmetric-session=");
    expect(setCookie).toContain("Path=/");
    expect(setCookie).toContain("Path=/api/auth");
    expect(setCookie).toContain("Max-Age=0");
    expect(setCookie).toContain("Secure");
  });
});
