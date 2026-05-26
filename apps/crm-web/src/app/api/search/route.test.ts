import { NextRequest } from "next/server";
import { describe, expect, it } from "vitest";

import { createSearchProxyHeaders, createSearchProxyUrl } from "./route";

describe("CRM search proxy", () => {
  it("forwards only supported search query parameters", () => {
    const request = new NextRequest(
      "http://localhost:7006/api/search?query=customer&sources=Crm,Tools&tags=crm&tags=customer&locale=en-US&debug=true",
    );

    const target = new URL(createSearchProxyUrl(request, "http://localhost:5030/"));

    expect(target.origin).toBe("http://localhost:5030");
    expect(target.pathname).toBe("/api/v1/search");
    expect(target.searchParams.get("query")).toBe("customer");
    expect(target.searchParams.get("sources")).toBe("Crm,Tools");
    expect(target.searchParams.getAll("tags")).toEqual(["crm", "customer"]);
    expect(target.searchParams.get("locale")).toBe("en-US");
    expect(target.searchParams.has("debug")).toBe(false);
  });

  it("forwards bearer authorization and correlation headers", () => {
    const headers = createSearchProxyHeaders(
      {
        bearerToken: "access-token",
      },
      "correlation-1",
    );

    expect(headers.get("accept")).toBe("application/json");
    expect(headers.get("authorization")).toBe("Bearer access-token");
    expect(headers.get("x-correlation-id")).toBe("correlation-1");
  });

  it("omits authorization when auth context is missing", () => {
    const headers = createSearchProxyHeaders(undefined, undefined);
    expect(headers.get("accept")).toBe("application/json");
    expect(headers.has("authorization")).toBe(false);
  });
});
