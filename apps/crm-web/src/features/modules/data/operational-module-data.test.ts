import { describe, expect, it } from "vitest";

import { getOperationalModuleConfig, summarizeOperationalPayload } from "./operational-module-data";

describe("operational module data", () => {
  it("summarizes paged CRM API payloads with a compact preview", () => {
    const summary = summarizeOperationalPayload({
      items: [
        { id: "campaign-1", name: "Spring launch", status: "Draft", ignored: "x" },
        { id: "campaign-2", name: "Retention", status: "Scheduled" },
      ],
      totalCount: 12,
      pageNumber: 1,
      pageSize: 2,
    });

    expect(summary.kind).toBe("paged_collection");
    expect(summary.count).toBe(12);
    expect(summary.preview.columns).toEqual(["id", "name", "status", "ignored"]);
    expect(summary.preview.rows[0]).toMatchObject({
      id: "campaign-1",
      name: "Spring launch",
      status: "Draft",
    });
  });

  it("registers shell-only CRM modules as operational workspaces", () => {
    expect(getOperationalModuleConfig("/marketing")?.endpoints.length).toBeGreaterThan(1);
    expect(getOperationalModuleConfig("/sales-forecasting")?.endpoints.length).toBeGreaterThan(1);
    expect(getOperationalModuleConfig("/settings")).toBeNull();
  });
});
