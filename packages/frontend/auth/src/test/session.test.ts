import { describe, expect, it, vi } from "vitest";

import {
  createAuthCookieDescriptors,
  createAuthServiceHeaders,
  getAccessTokenFromCookieHeader,
  getSetCookieHeaders,
  logoutFromAuthService,
  mapAccountOverviewToCurrentUserProfile,
  mapSessionStatusToSafeSession,
  resolveCurrentSession,
  shouldUseSecureAuthCookie,
} from "../index";

describe("shared session helper", () => {
  it("maps authenticated responses to a safe session DTO", () => {
    const session = mapSessionStatusToSafeSession({
      tenantId: "tenant-1",
      userId: "user-1",
      sessionId: "session-1",
      email: "ada@example.com",
      roles: ["owner"],
      permissions: ["customers.read"],
      accountStatus: "active",
      emailConfirmed: true,
      mfaVerifiedAt: null,
      accessToken: "must-not-leak",
      refreshToken: "must-not-leak",
    });

    expect(session.isAuthenticated).toBe(true);
    if (!session.isAuthenticated) {
      throw new Error("Expected authenticated session.");
    }

    expect(session.user.userId).toBe("user-1");
    expect(session.user.email).toBe("ada@example.com");
    expect(session.user.permissions).toEqual(["customers.read"]);
    expect(JSON.stringify(session)).not.toContain("must-not-leak");
    expect("accessToken" in session.user).toBe(false);
    expect("refreshToken" in session.user).toBe(false);
  });

  it("returns unauthenticated when no access cookie is present", async () => {
    const fetchImpl = vi.fn<typeof fetch>();

    await expect(
      resolveCurrentSession({
        authBaseUrl: "https://api.netmetric.net",
        cookieHeader: "nm_culture=en-US",
        fetchImpl,
      }),
    ).resolves.toEqual({ isAuthenticated: false });

    expect(fetchImpl).not.toHaveBeenCalled();
  });

  it("resolves authenticated session status through the configured endpoint", async () => {
    const fetchImpl = vi.fn<typeof fetch>().mockResolvedValue(
      Response.json({
        tenantId: "tenant-1",
        userId: "user-1",
        sessionId: "session-1",
        email: "ada@example.com",
        roles: [],
        permissions: [],
        accountStatus: "active",
        emailConfirmed: true,
        mfaVerifiedAt: null,
      }),
    );

    const session = await resolveCurrentSession({
      authBaseUrl: "https://api.netmetric.net/",
      cookieHeader: "__Secure-netmetric-access=access-token",
      correlationId: "correlation-1",
      fetchImpl,
    });

    expect(session.isAuthenticated).toBe(true);
    expect(fetchImpl).toHaveBeenCalledWith(
      "https://api.netmetric.net/api/auth/session-status",
      expect.objectContaining({
        method: "GET",
        cache: "no-store",
        redirect: "manual",
      }),
    );

    const headers = fetchImpl.mock.calls[0]?.[1]?.headers;
    expect(headers).toBeInstanceOf(Headers);
    expect((headers as Headers).get("authorization")).toBe("Bearer access-token");
    expect((headers as Headers).get("cookie")).toBe("__Secure-netmetric-access=access-token");
    expect((headers as Headers).get("x-correlation-id")).toBe("correlation-1");
    expect((headers as Headers).get("x-request-id")).toBe("correlation-1");
  });

  it("handles unavailable auth service safely", async () => {
    const fetchImpl = vi.fn<typeof fetch>().mockRejectedValue(new Error("offline"));

    await expect(
      resolveCurrentSession({
        authBaseUrl: "https://api.netmetric.net",
        cookieHeader: "__Secure-netmetric-access=access-token",
        fetchImpl,
      }),
    ).resolves.toEqual({ isAuthenticated: false, unavailable: true });
  });

  it("supports configured and fallback access cookie names", () => {
    expect(
      getAccessTokenFromCookieHeader("custom_access=one; __Secure-netmetric-access=two", [
        "custom_access",
      ]),
    ).toBe("one");
    expect(getAccessTokenFromCookieHeader("__Secure-nm_access=legacy")).toBe("legacy");
    expect(getAccessTokenFromCookieHeader("netmetric-access=dev-token")).toBe("dev-token");
  });

  it("builds Auth API headers with forwarded cookies and optional request metadata", () => {
    const headers = createAuthServiceHeaders({
      cookieHeader: "__Secure-netmetric-access=access-token; other=value",
      correlationId: "correlation-1",
      origin: "https://account.netmetric.net",
      referer: "https://account.netmetric.net/profile",
      userAgent: "vitest",
      contentType: "application/json",
    });

    expect(headers.get("authorization")).toBe("Bearer access-token");
    expect(headers.get("cookie")).toBe("__Secure-netmetric-access=access-token; other=value");
    expect(headers.get("origin")).toBe("https://account.netmetric.net");
    expect(headers.get("referer")).toBe("https://account.netmetric.net/profile");
    expect(headers.get("user-agent")).toBe("vitest");
    expect(headers.get("content-type")).toBe("application/json");
    expect(headers.get("x-request-id")).toBe("correlation-1");
  });

  it("deduplicates auth cookies to clear across root and auth paths", () => {
    expect(
      createAuthCookieDescriptors({
        accessCookieName: "__Secure-netmetric-access",
        refreshCookieName: "__Secure-netmetric-refresh",
        sessionCookieName: "__Secure-netmetric-session",
      }),
    ).toEqual([
      { name: "__Secure-netmetric-access", path: "/" },
      { name: "netmetric-access", path: "/" },
      { name: "__Secure-nm_access", path: "/" },
      { name: "__Secure-netmetric-refresh", path: "/" },
      { name: "netmetric-refresh", path: "/" },
      { name: "__Secure-netmetric-session", path: "/" },
      { name: "netmetric-session", path: "/" },
      { name: "__Secure-netmetric-refresh", path: "/api/auth" },
      { name: "netmetric-refresh", path: "/api/auth" },
      { name: "__Secure-netmetric-session", path: "/api/auth" },
      { name: "netmetric-session", path: "/api/auth" },
    ]);
  });

  it("keeps prefixed auth cookie clears compliant with browser prefix rules", () => {
    expect(shouldUseSecureAuthCookie("__Secure-netmetric-access", false)).toBe(true);
    expect(shouldUseSecureAuthCookie("__Host-netmetric-access", false)).toBe(true);
    expect(shouldUseSecureAuthCookie("netmetric-access", false)).toBe(false);
    expect(shouldUseSecureAuthCookie("netmetric-access", true)).toBe(true);
  });

  it("reads multiple set-cookie headers when the runtime exposes getSetCookie", () => {
    const headers = new Headers() as Headers & { getSetCookie: () => string[] };
    headers.getSetCookie = () => ["a=1", "b=2"];

    expect(getSetCookieHeaders(headers)).toEqual(["a=1", "b=2"]);
  });

  it("maps account overview data to a reusable current-user profile summary", () => {
    expect(
      mapAccountOverviewToCurrentUserProfile({
        displayName: "Ada Lovelace",
        avatarUrl: "https://cdn.netmetric.test/ada.png",
        organizations: [
          {
            tenantId: "tenant-1",
            organizationName: "NetMetric Labs",
            organizationSlug: "labs",
            roles: ["tenant-owner"],
            isDefault: true,
          },
        ],
      }),
    ).toEqual({
      displayName: "Ada Lovelace",
      avatarUrl: "https://cdn.netmetric.test/ada.png",
      activeWorkspace: {
        tenantId: "tenant-1",
        organizationName: "NetMetric Labs",
        organizationSlug: "labs",
        role: "tenant-owner",
        isDefault: true,
      },
      workspaces: [
        {
          tenantId: "tenant-1",
          organizationName: "NetMetric Labs",
          organizationSlug: "labs",
          role: "tenant-owner",
          isDefault: true,
        },
      ],
    });
  });

  it("posts centralized logout with forwarded cookies and returns upstream cookie clears", async () => {
    const fetchImpl = vi.fn<typeof fetch>().mockResolvedValue(
      new Response(null, {
        status: 204,
        headers: {
          "set-cookie": "__Secure-netmetric-access=; Max-Age=0",
        },
      }),
    );

    const result = await logoutFromAuthService({
      authBaseUrl: "https://api.netmetric.net/",
      cookieHeader:
        "__Secure-netmetric-access=access-token; __Secure-netmetric-refresh=refresh-token",
      origin: "https://crm.netmetric.net",
      referer: "https://crm.netmetric.net/customers",
      userAgent: "vitest",
      correlationId: "correlation-1",
      fetchImpl,
    });

    expect(result).toEqual({
      ok: true,
      status: 204,
      unavailable: false,
      setCookieHeaders: ["__Secure-netmetric-access=; Max-Age=0"],
    });
    expect(fetchImpl).toHaveBeenCalledWith(
      "https://api.netmetric.net/api/auth/logout",
      expect.objectContaining({
        method: "POST",
        body: "{}",
        cache: "no-store",
        redirect: "manual",
      }),
    );

    const headers = fetchImpl.mock.calls[0]?.[1]?.headers;
    expect(headers).toBeInstanceOf(Headers);
    expect((headers as Headers).get("cookie")).toContain(
      "__Secure-netmetric-refresh=refresh-token",
    );
    expect((headers as Headers).get("origin")).toBe("https://crm.netmetric.net");
    expect((headers as Headers).get("referer")).toBe("https://crm.netmetric.net/customers");
    expect((headers as Headers).get("content-type")).toBe("application/json");
  });
});
