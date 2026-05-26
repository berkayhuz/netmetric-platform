import { beforeEach, describe, expect, it, vi } from "vitest";
import { randomUUID } from "node:crypto";

const mocks = vi.hoisted(() => ({
  accessToken: undefined as string | undefined,
  redirect: vi.fn((url: string) => {
    throw Object.assign(new Error("NEXT_REDIRECT"), { url });
  }),
}));

vi.mock("next/headers", () => ({
  cookies: async () => ({
    get: (name: string) =>
      name === "__Secure-netmetric-access" && mocks.accessToken
        ? { value: mocks.accessToken }
        : undefined,
  }),
  headers: async () => ({
    get: (name: string) =>
      name === "cookie" && mocks.accessToken
        ? `__Secure-netmetric-access=${mocks.accessToken}`
        : null,
  }),
}));

vi.mock("next/navigation", () => ({
  redirect: mocks.redirect,
}));

import { getOptionalCrmShellSession, isPublicCrmPath, validateCrmSession } from "./crm-session";

function sessionStatusResponse(
  permissions: readonly string[],
  overrides: Record<string, unknown> = {},
) {
  return new Response(
    JSON.stringify({
      tenantId: "test-tenant-id",
      userId: "test-user-id",
      sessionId: "test-session-id",
      email: "ada@example.com",
      roles: ["tenant-owner"],
      permissions,
      accountStatus: "active",
      emailConfirmed: true,
      mfaVerifiedAt: null,
      ...overrides,
    }),
    { status: 200, headers: { "content-type": "application/json" } },
  );
}

describe("CRM session route classification", () => {
  beforeEach(() => {
    mocks.accessToken = undefined;
    mocks.redirect.mockClear();
    vi.stubGlobal("fetch", vi.fn());
  });

  it("leaves status pages public for unauthenticated redirects", () => {
    expect(isPublicCrmPath("/access-denied")).toBe(true);
    expect(isPublicCrmPath("/service-unavailable")).toBe(true);
    expect(isPublicCrmPath("/retry-later")).toBe(true);
  });

  it("requires the centralized guard for protected CRM paths", () => {
    expect(isPublicCrmPath("/customers")).toBe(false);
    expect(isPublicCrmPath("/dashboard")).toBe(false);
  });

  it("redirects users without an access token to login", async () => {
    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: expect.stringContaining("/login"),
    });
    expect(fetch).not.toHaveBeenCalled();
  });

  it("resets stale auth cookies before sending invalid sessions back to login", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch).mockResolvedValueOnce(new Response(null, { status: 401 }));

    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: expect.stringContaining("/auth/session-reset"),
    });
    expect(mocks.redirect.mock.calls[0]?.[0]).toContain(
      "returnUrl=http%3A%2F%2Flocalhost%3A7006%2Fcustomers",
    );
  });

  it("redirects unauthorized sessions safely", async () => {
    mocks.accessToken = randomUUID();

    vi.mocked(fetch).mockResolvedValueOnce(new Response(null, { status: 403 }));
    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: "/access-denied",
    });
  });

  it("requires route capabilities even when session introspection succeeds", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch).mockResolvedValueOnce(sessionStatusResponse(["customers.read"]));

    await expect(validateCrmSession("/customers/new")).rejects.toMatchObject({
      url: "/access-denied",
    });
  });

  it("returns a CRM session for valid introspection and route capability", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch)
      .mockResolvedValueOnce(sessionStatusResponse(["customers.read"]))
      .mockResolvedValue(new Response(null, { status: 404 }));

    const session = await validateCrmSession("/customers");

    expect(session.accessToken).toBe(mocks.accessToken);
    expect(session.capabilities["customers.read"]).toBe(true);
    expect(session.profile.tenantId).toBe("test-tenant-id");
    expect(session.profile.email).toBe("ada@example.com");
    expect(fetch).toHaveBeenNthCalledWith(
      1,
      "http://localhost:5030/api/auth/session-status",
      expect.objectContaining({
        method: "GET",
      }),
    );
  });

  it("hydrates CRM shell user data from the shared account API on first render", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch)
      .mockResolvedValueOnce(sessionStatusResponse(["customers.read"]))
      .mockResolvedValueOnce(
        Response.json({
          displayName: "Ada Lovelace",
          avatarUrl: "https://cdn.netmetric.test/ada.png",
          organizations: [
            {
              tenantId: "test-tenant-id",
              organizationName: "NetMetric Labs",
              organizationSlug: "netmetric-labs",
              roles: ["tenant-owner"],
              isDefault: true,
            },
          ],
        }),
      )
      .mockResolvedValueOnce(
        Response.json({
          items: [],
          unreadCount: 0,
        }),
      )
      .mockResolvedValueOnce(Response.json({ faviconUrl: "https://cdn.netmetric.test/icon.ico" }));

    const session = await validateCrmSession("/customers");

    expect(session.shellUser).toEqual({
      displayName: "Ada Lovelace",
      email: "ada@example.com",
      avatarUrl: "https://cdn.netmetric.test/ada.png",
      workspaceName: "NetMetric Labs",
      sessionStatus: "authenticated",
    });
    expect(session.faviconUrl).toBe("https://cdn.netmetric.test/icon.ico");
    expect(fetch).toHaveBeenNthCalledWith(
      2,
      "http://localhost:5030/api/v1/account/overview",
      expect.objectContaining({
        method: "GET",
      }),
    );
    expect(fetch).toHaveBeenNthCalledWith(
      3,
      "http://localhost:5030/api/v1/account/notifications",
      expect.objectContaining({
        method: "GET",
      }),
    );
  });

  it("hydrates optional shell session on public status pages without route redirects", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch)
      .mockResolvedValueOnce(sessionStatusResponse(["customers.read"]))
      .mockResolvedValueOnce(
        Response.json({
          displayName: "Ada Lovelace",
          avatarUrl: "https://cdn.netmetric.test/ada.png",
          organizations: [
            {
              tenantId: "test-tenant-id",
              organizationName: "NetMetric Labs",
              isDefault: true,
            },
          ],
        }),
      )
      .mockResolvedValueOnce(
        Response.json({
          items: [],
          unreadCount: 0,
        }),
      )
      .mockResolvedValueOnce(Response.json({ faviconUrl: null }));

    const session = await getOptionalCrmShellSession();

    expect(session?.shellUser.displayName).toBe("Ada Lovelace");
    expect(session?.shellUser.workspaceName).toBe("NetMetric Labs");
    expect(mocks.redirect).not.toHaveBeenCalled();
  });

  it("redirects disabled or unconfirmed profiles away from CRM", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch).mockResolvedValueOnce(
      sessionStatusResponse(["customers.read"], { accountStatus: "disabled" }),
    );

    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: "/access-denied",
    });

    vi.mocked(fetch).mockResolvedValueOnce(
      sessionStatusResponse(["customers.read"], { emailConfirmed: false }),
    );

    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: "/access-denied",
    });
  });

  it("redirects missing tenant context safely", async () => {
    mocks.accessToken = randomUUID();
    vi.mocked(fetch).mockResolvedValueOnce(
      sessionStatusResponse(["customers.read"], { tenantId: "" }),
    );

    await expect(validateCrmSession("/customers")).rejects.toMatchObject({
      url: "/access-denied",
    });
  });
});
