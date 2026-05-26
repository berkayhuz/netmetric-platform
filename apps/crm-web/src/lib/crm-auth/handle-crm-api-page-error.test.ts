import { describe, expect, it, vi } from "vitest";

const mocks = vi.hoisted(() => ({
  redirect: vi.fn((url: string) => {
    throw Object.assign(new Error("NEXT_REDIRECT"), { url });
  }),
}));

vi.mock("next/navigation", () => ({
  notFound: vi.fn(() => {
    throw new Error("NEXT_NOT_FOUND");
  }),
  redirect: mocks.redirect,
}));

import { CrmApiError } from "@/lib/crm-api";

import { handleCrmApiPageError } from "./handle-crm-api-page-error";

describe("handleCrmApiPageError", () => {
  it("resets CRM auth cookies instead of redirecting API 401 errors back to login", () => {
    expect(() =>
      handleCrmApiPageError(
        new CrmApiError({
          message: "Unauthorized",
          status: 401,
        }),
        "/dashboard",
      ),
    ).toThrow("NEXT_REDIRECT");

    expect(mocks.redirect).toHaveBeenCalledWith(
      "/auth/session-reset?returnUrl=http%3A%2F%2Flocalhost%3A7006%2Fdashboard",
    );
  });
});
