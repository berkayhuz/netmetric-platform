import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const contactsPagePath = path.resolve(__dirname, "./page.tsx");

describe("contacts page table path wiring", () => {
  it("uses the single CrmEntityListPage path without rollout flags", () => {
    const source = fs.readFileSync(contactsPagePath, "utf8");

    expect(source).not.toContain("shouldUseCrmDataTableAdapterRoute(");
    expect(source).not.toContain("tableAdapterPreview");
    expect(source).not.toContain("useDataTableAdapterPreview");
    expect(source).toContain("<CrmEntityListPage");
    expect(source).toContain('routePath="/contacts"');
    expect(source).toContain('actionPath="/contacts"');
  });
});
