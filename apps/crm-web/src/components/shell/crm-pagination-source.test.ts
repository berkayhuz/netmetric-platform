import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const filePath = path.resolve(__dirname, "./crm-pagination.tsx");

describe("crm pagination server-link behavior", () => {
  it("keeps crm query/href ownership while delegating visuals to shared link pagination", () => {
    const source = fs.readFileSync(filePath, "utf8");

    expect(source).toContain("new URL(`http://localhost${basePath}`)");
    expect(source).toContain('if (key === "page")');
    expect(source).toContain("baseUrl.searchParams.set(key, value)");
    expect(source).toContain("withPage(baseUrl, prevPage)");
    expect(source).toContain("withPage(baseUrl, nextPage)");
    expect(source).toContain("<LinkPagination");
    expect(source).toContain("items={items}");
  });
});
