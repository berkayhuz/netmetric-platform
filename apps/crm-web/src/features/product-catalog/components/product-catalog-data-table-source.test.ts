import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const tablePath = path.resolve(__dirname, "./product-catalog-data-table.tsx");

describe("product catalog data-table source contract", () => {
  it("keeps product-catalog specific table ownership and workflows in crm-web", () => {
    const source = fs.readFileSync(tablePath, "utf8");

    expect(source).toContain("<CrmRecordsTable");
    expect(source).not.toContain("<EntityDataTable");
    expect(source).toContain("downloadProductCatalogExportAction");
    expect(source).toContain("downloadProductCatalogTemplateAction");
    expect(source).toContain("bulkSetProductCatalogItemsActiveStateAction");
    expect(source).toContain("bulkDeleteProductCatalogItemsAction");
    expect(source).toContain("setSelectedIds");
    expect(source).toContain("setActionMessage");
    expect(source).toContain("router.refresh()");
    expect(source).toContain("mapProductCatalogRows");
    expect(source).toContain("buildProductCatalogColumns");
    expect(source).toContain("EntityTableInfoStrip");
  });
});
