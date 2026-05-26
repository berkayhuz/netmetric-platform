import { describe, expect, it } from "vitest";

import { CrmApiError, statusToCrmApiErrorKind } from "./crm-api-errors";

describe("CRM API error mapping", () => {
  it("maps standard HTTP statuses to safe CRM error kinds", () => {
    expect(statusToCrmApiErrorKind(401)).toBe("unauthorized");
    expect(statusToCrmApiErrorKind(403)).toBe("forbidden");
    expect(statusToCrmApiErrorKind(404)).toBe("not_found");
    expect(statusToCrmApiErrorKind(409)).toBe("conflict");
    expect(statusToCrmApiErrorKind(422)).toBe("validation");
    expect(statusToCrmApiErrorKind(500)).toBe("server_error");
  });

  it("does not expose backend 500 detail text through problem details", () => {
    const error = new CrmApiError({
      message: "System.InvalidOperationException: connection string leaked",
      status: 500,
      problem: {
        title: "System.InvalidOperationException",
        detail: "connection string leaked",
      },
    });

    expect(error.kind).toBe("server_error");
    expect(error.problem?.title).toBe("CRM service unavailable.");
    expect(error.problem?.detail).toBe("Please try again later.");
  });
});
