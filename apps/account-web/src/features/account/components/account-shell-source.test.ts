import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const shellClientPath = path.resolve(__dirname, "./account-shell-client.tsx");
const shellPath = path.resolve(__dirname, "./account-shell.tsx");

describe("Account shell shared UI integration", () => {
  it("uses localized account menu labels and avoids silent no-op actions", () => {
    const shellClientSource = fs.readFileSync(shellClientPath, "utf8");
    const shellSource = fs.readFileSync(shellPath, "utf8");

    expect(shellClientSource).toContain("<AppWorkspaceShell");
    expect(shellClientSource).toContain("useRouter");
    expect(shellClientSource).toContain("<CriticalRouteWarmup");
    expect(shellClientSource).toContain("router.prefetch(href)");
    expect(shellClientSource).toContain("<ClientRoutePerformanceReporter");
    expect(shellClientSource).toContain("accountRoutes");
    expect(shellClientSource).toContain("accountNavIcons");
    expect(shellClientSource).toContain("backgroundStyle={backgroundStyle}");
    expect(shellClientSource).toContain('tAccountClient("account.shell.userMenu.inviteUser")');
    expect(shellClientSource).toContain('tAccountClient("account.shell.userMenu.settings")');
    expect(shellClientSource).toContain('tAccountClient("account.shell.userMenu.support")');
    expect(shellClientSource).toContain('tAccountClient("account.shell.userMenu.createWorkspace")');
    expect(shellClientSource).toContain('tAccountClient("account.shell.userMenu.signOut")');
    expect(shellClientSource).toContain(
      'tAccountClient("account.shell.userMenu.unavailableAction")',
    );
    expect(shellClientSource).toContain("createSharedWorkspaceUserMenuActions");
    expect(shellClientSource).toContain("onSignOut: logout");
    expect(shellClientSource).toContain("secondaryText: workspaceName ??");
    expect(shellClientSource).not.toContain("onSelect: () => {}");
    expect(shellClientSource).not.toContain('"Invite user"');
    expect(shellClientSource).not.toContain('"Create workspace"');
    expect(shellClientSource).not.toContain('"Sign out"');
    expect(shellClientSource).toContain("notifications");
    expect(shellClientSource).toContain("<GlobalSearchDialog");
    expect(shellClientSource).toContain('searchEndpoint="/api/search"');
    expect(shellClientSource).toContain("defaultSources={accountSearchDefaultSources}");
    expect(shellClientSource).toContain("sourceOrder={accountSearchSourceOrder}");
    expect(shellClientSource).toContain("locale={locale}");
    expect(shellSource).toContain("locale={locale}");
    expect(shellClientSource).toContain(
      'const accountSearchDefaultSources = ["account", "tools", "public"] as const;',
    );
    expect(shellClientSource).toContain("onSearchFocus: openSearch");
    expect(shellClientSource).toContain("searchReadOnly");
    expect(shellClientSource).not.toContain("prefetch={false}");
    expect(shellClientSource).toContain('tAccountClient("account.shell.sidebar.collapse")');
    expect(shellClientSource).toContain('tAccountClient("account.shell.sidebar.expand")');
    expect(shellSource).toContain("loadAppBackgroundStyle");
  });
});
