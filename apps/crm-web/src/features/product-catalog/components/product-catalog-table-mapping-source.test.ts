import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const sourcePath = path.resolve(__dirname, "./product-catalog-table-mapping.ts");

describe("product catalog table mapping source contract", () => {
  it("stays pure and local without workflow or shared-ui coupling", () => {
    const source = fs.readFileSync(sourcePath, "utf8");

    expect(source).toContain("mapProductCatalogRows");
    expect(source).toContain("buildProductCatalogColumns");
    expect(source).toContain("formatProductCatalogCurrency");
    expect(source).not.toContain("product-catalog-mutation-actions");
    expect(source).not.toContain("downloadProductCatalogExportAction");
    expect(source).not.toContain("bulkDeleteProductCatalogItemsAction");
    expect(source).not.toContain("@netmetric/ui/client");
    expect(source).not.toContain("EntityDataTable");
  });
});
