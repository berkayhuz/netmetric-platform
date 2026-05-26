import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const adapterPath = path.resolve(__dirname, "./crm-data-table-adapter.tsx");

describe("crm data table adapter preview chrome wiring", () => {
  it("routes toolbar/search/filter chrome through CrmEntityDataTable", () => {
    const source = fs.readFileSync(adapterPath, "utf8");

    expect(source).toContain("<CrmEntityDataTable");
    expect(source).toContain("enableSearch");
    expect(source).toContain("enableFilters");
    expect(source).toContain("facetedFilters={facetedFilters}");
    expect(source).toContain("enableColumnVisibility");
    expect(source).toContain("enablePagination={false}");
    expect(source).toContain("meta: { disableReorder: true }");
    expect(source).toContain("renderToolbar={({ table }) =>");
    expect(source).toContain("loading={loading}");
    expect(source).toContain("error={error}");
    expect(source).not.toContain("<CrmListFilterBar");
  });
});
