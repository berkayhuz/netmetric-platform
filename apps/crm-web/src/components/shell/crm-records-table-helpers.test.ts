import { describe, expect, it } from "vitest";

import {
  compareCrmRecordValues,
  formatCrmRecordCellDisplayValue,
  getCrmRecordBadgeClassName,
  getCrmRecordCellValue,
  getCrmRecordSearchHaystack,
  resolveCrmRecordFilterValue,
  shouldUseCrmRecordBadgeColumn,
} from "./crm-records-table-helpers";
import type { CrmRecordsTableRow } from "./crm-records-table";

describe("crm-records-table helpers parity", () => {
  it("prefers explicit searchText over cells/descriptions", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { name: "Alice", status: "Active" },
      descriptions: { name: "Ignored description" },
      searchText: "custom haystack",
    };

    expect(getCrmRecordSearchHaystack(row)).toBe("custom haystack");
  });

  it("builds search haystack from cells and descriptions when searchText is absent", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { name: "Alice", status: "Active" },
      descriptions: { name: "Primary contact", note: undefined },
    };

    expect(getCrmRecordSearchHaystack(row)).toContain("Alice");
    expect(getCrmRecordSearchHaystack(row)).toContain("Active");
    expect(getCrmRecordSearchHaystack(row)).toContain("Primary contact");
  });

  it("resolves filter value from filterValues first and falls back to cells", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { status: "CellStatus" },
      filterValues: { status: "FilterStatus" },
    };
    const fallbackRow: CrmRecordsTableRow = {
      id: "2",
      cells: { status: "CellStatus" },
    };

    expect(resolveCrmRecordFilterValue(row, "status")).toBe("FilterStatus");
    expect(resolveCrmRecordFilterValue(fallbackRow, "status")).toBe("CellStatus");
  });

  it("keeps numeric and text sort behavior parity", () => {
    expect(compareCrmRecordValues("2", "10", "asc")).toBeLessThan(0);
    expect(compareCrmRecordValues("2", "10", "desc")).toBeGreaterThan(0);
    expect(compareCrmRecordValues("A2", "a10", "asc")).toBeLessThan(0);
  });

  it("keeps empty fallback and normal string passthrough behavior", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { empty: "", value: "hello" },
    };

    expect(getCrmRecordCellValue(row, "empty")).toBe("-");
    expect(getCrmRecordCellValue(row, "value")).toBe("hello");
  });

  it("formats date-like values and preserves invalid date strings", () => {
    const formattedDate = formatCrmRecordCellDisplayValue("2026-05-15");
    const formattedDateTime = formatCrmRecordCellDisplayValue("2026-05-15T21:30:00Z");
    const invalidDateTime = formatCrmRecordCellDisplayValue("2026-99-99T21:30:00Z");

    expect(formattedDate).not.toBe("2026-05-15");
    expect(formattedDateTime).not.toBe("2026-05-15T21:30:00Z");
    expect(invalidDateTime).toBe("2026-99-99T21:30:00Z");
  });

  it("keeps badge column and badge class heuristics", () => {
    expect(shouldUseCrmRecordBadgeColumn({ key: "status", header: "Status" })).toBe(true);
    expect(shouldUseCrmRecordBadgeColumn({ key: "name", header: "Name" })).toBe(false);
    expect(getCrmRecordBadgeClassName("active")).toContain("emerald");
    expect(getCrmRecordBadgeClassName("inactive")).toContain("muted");
    expect(getCrmRecordBadgeClassName("plain-text")).toBeUndefined();
  });
});
