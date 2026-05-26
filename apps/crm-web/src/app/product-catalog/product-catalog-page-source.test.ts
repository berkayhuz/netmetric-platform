import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const pagePath = path.resolve(__dirname, "./page.tsx");

describe("product catalog page source contract", () => {
  it("keeps product-catalog default route wired to ProductCatalogDataTable without preview migration gates", () => {
    const source = fs.readFileSync(pagePath, "utf8");

    expect(source).toContain("<ProductCatalogDataTable");
    expect(source).toContain("toProductCatalogListQuery(");
    expect(source).toContain("getProductCatalogData(");
    expect(source).toContain("getProductCatalogLookups(");
    expect(source).toContain("getProductCatalogMeta(");
    expect(source).toContain("getProductCatalogStats(");
    expect(source).not.toContain("shouldUseCrmDataTableAdapterRoute(");
    expect(source).not.toContain("EntityDataTable");
    expect(source).not.toContain("productCatalogTablePreview");
  });
});
