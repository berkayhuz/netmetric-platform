/// <reference types="node" />

import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const repoRoot = path.resolve(__dirname, "../../../../..");

function read(relativePath: string): string {
  return fs.readFileSync(path.resolve(repoRoot, relativePath), "utf8");
}

function expectMainContentLandmark(source: string): void {
  expect(source).toMatch(/<main\s+id="main-content"/);
}

describe("app shell accessibility contract", () => {
  it("keeps skip-link and main landmark patterns across shell implementations", () => {
    const authLayout = read("apps/auth-web/src/app/layout.tsx");
    const sharedAppShell = read("packages/frontend/ui/src/components/shell/app-shell.tsx");
    const accountShell = read(
      "apps/account-web/src/features/account/components/account-shell-client.tsx",
    );
    const crmShell = read("apps/crm-web/src/components/shell/crm-shell.tsx");
    const toolsLayout = read("apps/tools-web/src/app/layout.tsx");
    const publicLayout = read("apps/public-web/src/app/layout.tsx");

    expect(authLayout).toContain('href="#main-content"');
    expectMainContentLandmark(authLayout);

    expect(sharedAppShell).toContain('href="#main-content"');
    expectMainContentLandmark(sharedAppShell);

    expect(accountShell).toContain("<AppWorkspaceShell");

    expect(crmShell).toContain("<AppWorkspaceShell");

    expect(toolsLayout).toContain('href="#main-content"');
    expectMainContentLandmark(toolsLayout);

    expect(publicLayout).toContain('href="#main-content"');
    expectMainContentLandmark(publicLayout);
  });
});
