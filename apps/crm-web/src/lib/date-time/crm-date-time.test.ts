import { describe, expect, it } from "vitest";

import { formatCrmDate, formatCrmDateTime } from "./crm-date-time";

describe("CRM date/time formatting", () => {
  it("applies user timezone and date format preferences", () => {
    expect(
      formatCrmDateTime("2026-05-15T21:30:00Z", {
        locale: "en-GB",
        timeZone: "Europe/Istanbul",
        dateFormat: "dd/MM/yyyy",
      }),
    ).toContain("16/05/2026");
  });

  it("falls back safely for missing or invalid values", () => {
    expect(
      formatCrmDate(null, {
        locale: "en-US",
        timeZone: "UTC",
        dateFormat: "yyyy-MM-dd",
      }),
    ).toBe("-");

    expect(
      formatCrmDate("2026-05-15T10:30:00Z", {
        locale: "en-US",
        timeZone: "Invalid/Zone",
        dateFormat: "bad",
      }),
    ).toBe("2026-05-15T10:30:00.000Z");
  });
});
