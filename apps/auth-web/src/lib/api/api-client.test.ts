import { describe, expect, it } from "vitest";

describe("Auth API request headers", () => {
  it("forwards browser cookies and correlation id during server-side API calls", async () => {
    process.env.NEXT_PUBLIC_APP_ORIGIN = "http://localhost:7002";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "http://localhost:5030";
    const { createApiRequestHeaders } = await import("./api-client");

    const headers = await createApiRequestHeaders(
      undefined,
      new Headers({
        cookie: "__Secure-netmetric-access=access-token; __Secure-netmetric-refresh=refresh-token",
        "x-correlation-id": "correlation-1",
      }),
    );

    expect(headers.get("cookie")).toBe(
      "__Secure-netmetric-access=access-token; __Secure-netmetric-refresh=refresh-token",
    );
    expect(headers.get("authorization")).toBe("Bearer access-token");
    expect(headers.get("x-request-id")).toBe("correlation-1");
  });

  it("does not overwrite explicitly provided proxy headers", async () => {
    process.env.NEXT_PUBLIC_APP_ORIGIN = "http://localhost:7002";
    process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL = "http://localhost:5030";
    const { createApiRequestHeaders } = await import("./api-client");

    const headers = await createApiRequestHeaders(
      {
        cookie: "manual_cookie=value",
        "x-request-id": "manual-request",
      },
      new Headers({
        cookie: "__Secure-netmetric-access=browser-token",
        "x-correlation-id": "browser-correlation",
      }),
    );

    expect(headers.get("cookie")).toBe("manual_cookie=value");
    expect(headers.get("x-request-id")).toBe("manual-request");
  });
});
