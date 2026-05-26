import { describe, expect, it } from "vitest";

import {
  buildProductCatalogColumns,
  formatProductCatalogCurrency,
  mapProductCatalogRows,
} from "./product-catalog-table-mapping";

describe("product catalog table mapping helpers", () => {
  const labels = {
    actions: "Actions",
    active: "Active",
    addProduct: "Add product",
    category: "Category",
    code: "Code",
    columns: "Columns",
    description: "Description",
    edit: "Edit",
    emptyDescription: "Empty desc",
    emptyTitle: "Empty",
    errorDescription: "Error desc",
    errorTitle: "Error",
    firstPage: "First",
    inactive: "Inactive",
    lastPage: "Last",
    manageCategories: "Manage categories",
    name: "Name",
    nextPage: "Next",
    noCategory: "No category",
    noPrice: "No price",
    page: "Page",
    previousPage: "Previous",
    price: "Price",
    refresh: "Refresh",
    reset: "Reset",
    rowsPerPage: "Rows",
    search: "Search",
    searchPlaceholder: "Search",
    selectedRows: "{count} selected",
    status: "Status",
    view: "View",
    viewColumns: "View columns",
  } as const;

  it("builds column descriptors from labels", () => {
    const columns = buildProductCatalogColumns(labels);
    expect(columns).toHaveLength(5);
    expect(columns[0]?.key).toBe("name");
    expect(columns[2]?.key).toBe("categoryName");
    expect(columns[4]?.key).toBe("isActive");
  });

  it("maps product dto rows into crm records rows", () => {
    const rows = mapProductCatalogRows({
      items: [
        {
          id: "p1",
          name: "Starter",
          code: "ST-1",
          categoryName: null,
          categoryCode: null,
          description: "Starter plan",
          unitPrice: 12,
          currencyCode: "USD",
          isActive: true,
        },
      ] as never,
      labels,
      locale: "en-US",
    });

    expect(rows).toHaveLength(1);
    expect(rows[0]?.href).toBe("/product-catalog/p1");
    expect(rows[0]?.cells.categoryName).toBe("No category");
    expect(rows[0]?.cells.isActive).toBe("Active");
    expect(rows[0]?.searchText).toContain("Starter");
  });

  it("formats currency with fallback for invalid currency code", () => {
    expect(formatProductCatalogCurrency(null, "USD", "en-US")).toBe("");
    expect(formatProductCatalogCurrency(10, "INVALID", "en-US")).toBe("10.00 INVALID");
  });
});
