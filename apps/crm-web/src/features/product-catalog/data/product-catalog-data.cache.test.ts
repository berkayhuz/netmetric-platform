import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const sourcePath = path.resolve(__dirname, "./product-catalog-data.ts");

describe("product catalog lookup cache", () => {
  it("keeps authenticated lookup reads behind React request cache to avoid duplicate refetches", () => {
    const source = fs.readFileSync(sourcePath, "utf8");

    expect(source).toContain('import { cache } from "react"');
    expect(source).toContain("const getProductCatalogLookupsCached = cache(");
    expect(source).toContain("return getProductCatalogLookupsCached(returnPath)");
  });
});
