import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const filePath = path.resolve(__dirname, "./crm-entity-list-page.tsx");

describe("crm entity list table wiring", () => {
  it("uses a single CrmRecordsTable path without preview branches", () => {
    const source = fs.readFileSync(filePath, "utf8");

    expect(source).toContain("<CrmRecordsTable");
    expect(source).not.toContain("<CrmDataTableAdapter");
    expect(source).not.toContain("useDataTableAdapterPreview");
    expect(source).not.toContain("loading?: boolean;");
    expect(source).not.toContain("error?: unknown;");
  });
});
