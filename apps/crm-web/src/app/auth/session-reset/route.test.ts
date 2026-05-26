import { describe, expect, it, vi } from "vitest";
import { NextRequest } from "next/server";

import { GET } from "./route";

describe("CRM auth session reset", () => {
  it("clears auth cookies and redirects to auth login with a safe CRM return url", () => {
    vi.stubEnv("NEXT_PUBLIC_NETMETRIC_COOKIE_DOMAIN", "");
    vi.stubEnv("NEXT_PUBLIC_CRM_URL", "http://localhost:7006");
    vi.stubEnv("NEXT_PUBLIC_AUTH_URL", "http://localhost:7002");

    const request = new NextRequest(
      "http://localhost:7006/auth/session-reset?returnUrl=http%3A%2F%2Flocalhost%3A7006%2Fcustomers",
      {
        headers: {
          cookie:
            "netmetric-access=stale-token; __Secure-netmetric-access=stale-secure-token; netmetric-refresh=refresh-token; netmetric-session=session-id",
        },
      },
    );

    const response = GET(request);
    const location = response.headers.get("location");
    const setCookie = response.headers.get("set-cookie") ?? "";

    expect(response.status).toBe(307);
    expect(location).toBe(
      "http://localhost:7002/login?returnUrl=http%3A%2F%2Flocalhost%3A7006%2Fcustomers",
    );
    expect(response.headers.get("x-netmetric-auth-session-reset")).toBe("1");
    expect(setCookie).toContain("netmetric-access=");
    expect(setCookie).toContain("__Secure-netmetric-access=");
    expect(setCookie).toContain("netmetric-refresh=");
    expect(setCookie).toContain("__Secure-netmetric-refresh=");
    expect(setCookie).toContain("netmetric-session=");
    expect(setCookie).toContain("__Secure-netmetric-session=");
    expect(setCookie).toContain("Max-Age=0");
  });
});
