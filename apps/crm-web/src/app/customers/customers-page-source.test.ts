import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const customersPagePath = path.resolve(__dirname, "./page.tsx");

describe("customers page special-route contract", () => {
  it("uses standard CrmEntityListPage path without rollout fallback branches", () => {
    const source = fs.readFileSync(customersPagePath, "utf8");

    expect(source).toContain("<CrmEntityListPage");
    expect(source).toContain('routePath="/customers"');
    expect(source).toContain('actionPath="/customers"');
    expect(source).toContain('detailBasePath="/customers"');
    expect(source).not.toContain("<CustomersListTable");
    expect(source).not.toContain("<CustomersEntityDataTablePreview");
    expect(source).not.toContain("<CrmPagination");
    expect(source).not.toContain("shouldUseCrmDataTableAdapterRoute(");
    expect(source).not.toContain("useDataTableAdapterPreview");
    expect(source).toContain('href="/customers/imports"');
  });
});
