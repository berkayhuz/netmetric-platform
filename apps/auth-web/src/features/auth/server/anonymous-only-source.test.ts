import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const authAppRoot = path.resolve(__dirname, "../../../app/(auth)");

describe("anonymous-only auth pages", () => {
  it("guards public credential recovery and entry pages from authenticated users", () => {
    for (const route of ["login", "register", "forgot-password", "reset-password"]) {
      const source = fs.readFileSync(path.join(authAppRoot, route, "page.tsx"), "utf8");

      expect(source).toContain("requireAnonymousAuthPage");
    }
  });
});
