import { describe, expect, it } from "vitest";

import {
  areAllVisibleCrmRowsSelected,
  createCrmColumnDescriptors,
  createCrmDataTableColumnMappings,
  createCrmDataTableFilterMappings,
  createCrmDataTableSearchConfig,
  createCrmDataTableSortConfig,
  createCrmDataTableRowData,
  createCrmFilterPredicate,
  createCrmGlobalFilterFn,
  createCrmSortingFn,
  getCrmSelectedVisibleCount,
  mapCrmFiltersForDataTable,
  mapCrmRowsForDataTable,
  resolveCrmCellRenderValue,
  sortCrmRows,
} from "./crm-data-table-adapter.helpers";
import type { CrmRecordsTableColumn, CrmRecordsTableRow } from "./crm-records-table";

describe("crm-data-table adapter helper draft parity", () => {
  it("keeps identity mapping for row data", () => {
    const rows: CrmRecordsTableRow[] = [
      { id: "1", cells: { name: "Alice" } },
      { id: "2", cells: { name: "Bob" } },
    ];

    expect(createCrmDataTableRowData(rows)).toEqual(rows);
    expect(mapCrmRowsForDataTable(rows)).toEqual(rows);
  });

  it("maps column metadata descriptors with badge heuristic", () => {
    const columns: CrmRecordsTableColumn[] = [
      { key: "status", header: "Status" },
      { key: "name", header: "Name", sortable: false },
    ];
    const mappings = createCrmDataTableColumnMappings(columns);

    expect(mappings).toHaveLength(2);
    expect((mappings[0]?.target as { badge?: boolean }).badge).toBe(true);
    expect((mappings[1]?.target as { sortable?: boolean }).sortable).toBe(false);
    expect(createCrmColumnDescriptors(columns)).toEqual(mappings);
  });

  it("matches global filter behavior via search haystack", () => {
    const filterFn = createCrmGlobalFilterFn();
    const rowWithSearchText: CrmRecordsTableRow = {
      id: "1",
      cells: { name: "Ignored" },
      searchText: "custom content",
    };
    const rowWithoutSearchText: CrmRecordsTableRow = {
      id: "2",
      cells: { name: "Alice" },
      descriptions: { name: "Primary contact" },
    };

    expect(filterFn(rowWithSearchText, "CUSTOM")).toBe(true);
    expect(filterFn(rowWithoutSearchText, "primary")).toBe(true);
    expect(filterFn(rowWithoutSearchText, "not-found")).toBe(false);
    expect(createCrmDataTableSearchConfig().globalFilterFn(rowWithoutSearchText, "alice")).toBe(
      true,
    );
  });

  it("keeps filter predicate all sentinel and filterValues fallback behavior", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { status: "CellStatus" },
      filterValues: { status: "FilterStatus" },
    };

    expect(createCrmFilterPredicate("status", "all")(row)).toBe(true);
    expect(createCrmFilterPredicate("status", "FilterStatus")(row)).toBe(true);
    expect(createCrmFilterPredicate("status", "CellStatus")(row)).toBe(false);
  });

  it("keeps sorting function parity with sortValues over cells fallback", () => {
    const sortByAmount = createCrmSortingFn("amount");
    const left: CrmRecordsTableRow = {
      id: "1",
      cells: { amount: "200" },
      sortValues: { amount: 2 },
    };
    const right: CrmRecordsTableRow = {
      id: "2",
      cells: { amount: "100" },
      sortValues: { amount: 10 },
    };

    expect(sortByAmount(left, right, "asc")).toBeLessThan(0);
    expect(sortByAmount(left, right, "desc")).toBeGreaterThan(0);
    expect(createCrmDataTableSortConfig("amount").sortingFn(left, right, "asc")).toBeLessThan(0);
    expect(sortCrmRows([left, right], { key: "amount", direction: "asc" })[0]?.id).toBe("1");
    expect(sortCrmRows([left, right], { key: "amount", direction: "desc" })[0]?.id).toBe("2");
  });

  it("keeps cell descriptor value/description/badge class behavior", () => {
    const row: CrmRecordsTableRow = {
      id: "1",
      cells: { status: "active", empty: "" },
      descriptions: { status: "Row description" },
    };

    const statusRender = resolveCrmCellRenderValue(row, "status");
    const emptyRender = resolveCrmCellRenderValue(row, "empty");

    expect(statusRender.value).toBe("active");
    expect(statusRender.description).toBe("Row description");
    expect(statusRender.badgeClassName).toContain("emerald");
    expect(emptyRender.value).toBe("-");
  });

  it("maps filter descriptors as-is", () => {
    const mapped = createCrmDataTableFilterMappings([
      {
        key: "status",
        label: "Status",
        allLabel: "All status",
        options: [
          { value: "active", label: "Active" },
          { value: "inactive", label: "Inactive" },
        ],
      },
    ]);

    expect(mapped[0]?.key).toBe("status");
    expect(mapped[0]?.allLabel).toBe("All status");
    expect(mapped[0]?.options).toHaveLength(2);
    expect(mapCrmFiltersForDataTable(mapped)).toEqual(mapped);
  });

  it("keeps selected visible count and all-visible selection scope behavior", () => {
    const visibleRows: CrmRecordsTableRow[] = [
      { id: "1", cells: { name: "A" } },
      { id: "2", cells: { name: "B" } },
      { id: "3", cells: { name: "C" } },
    ];
    const partiallySelected = new Set(["1", "4"]);
    const allVisibleSelected = new Set(["1", "2", "3", "4"]);

    expect(getCrmSelectedVisibleCount(visibleRows, partiallySelected)).toBe(1);
    expect(areAllVisibleCrmRowsSelected(visibleRows, partiallySelected)).toBe(false);
    expect(areAllVisibleCrmRowsSelected(visibleRows, allVisibleSelected)).toBe(true);
  });
});
