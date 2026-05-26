import { readFileSync } from "node:fs";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

describe("dashboard fallback copy", () => {
  it("keeps contact fallback text behind i18n keys", () => {
    const sourcePath = fileURLToPath(new URL("./dashboard-data.ts", import.meta.url));
    const source = readFileSync(sourcePath, "utf8");

    expect(source).not.toContain('"No contact info"');
    expect(source).toContain("crm.dashboard.noContactInfo");
  });
});
