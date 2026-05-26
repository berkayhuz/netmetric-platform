import fs from "node:fs";
import path from "node:path";
import { fileURLToPath } from "node:url";

import { describe, expect, it } from "vitest";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const shellPath = path.resolve(__dirname, "./crm-shell.tsx");

describe("CRM shell shared UI integration", () => {
  it("uses localized CRM menu labels and avoids silent no-op actions", () => {
    const source = fs.readFileSync(shellPath, "utf8");

    expect(source).toContain("<AppWorkspaceShell");
    expect(source).toContain("useRouter");
    expect(source).toContain("<CriticalRouteWarmup");
    expect(source).toContain("router.prefetch(href)");
    expect(source).toContain("shouldUsePlainNavigationLinks");
    expect(source).toContain("<a href={href}");
    expect(source).toContain("<ClientRoutePerformanceReporter");
    expect(source).toContain("backgroundStyle={backgroundStyle}");
    expect(source).toContain("icon: crmNavIcons[moduleItem.iconKey]");
    expect(source).toContain('tCrm("crm.shell.userMenu.inviteUser", locale)');
    expect(source).toContain('tCrm("crm.shell.userMenu.settings", locale)');
    expect(source).toContain('tCrm("crm.shell.userMenu.support", locale)');
    expect(source).toContain('tCrm("crm.shell.userMenu.createWorkspace", locale)');
    expect(source).toContain('tCrm("crm.shell.userMenu.signOut", locale)');
    expect(source).toContain('tCrm("crm.shell.userMenu.unavailableAction", locale)');
    expect(source).toContain('tCrm("crm.shell.userFallback", locale)');
    expect(source).toContain(
      'secondaryText: user.workspaceName ?? tCrm("crm.shell.workspace", locale)',
    );
    expect(source).toContain("createSharedWorkspaceUserMenuActions");
    expect(source).toContain("onSignOut: logout");
    expect(source).not.toContain("onSelect: () => {}");
    expect(source).not.toContain('"Invite user"');
    expect(source).not.toContain('"Create workspace"');
    expect(source).not.toContain('"Sign out"');
    expect(source).not.toContain('"CRM user"');
    expect(source).toContain('tCrm("crm.shell.notifications.label", locale)');
    expect(source).toContain("<GlobalSearchDialog");
    expect(source).toContain('searchEndpoint="/api/search"');
    expect(source).toContain("defaultSources={crmSearchDefaultSources}");
    expect(source).toContain("sourceOrder={crmSearchSourceOrder}");
    expect(source).toContain("locale={locale}");
    expect(source).toContain(
      'const crmSearchDefaultSources = ["crm", "tools", "public"] as const;',
    );
    expect(source).toContain("onSearchFocus: openSearch");
    expect(source).toContain("searchReadOnly");
    expect(source).toContain('viewAllHref: "/notifications"');
    expect(source).toContain('viewAllLabel: tCrm("crm.shell.notifications.viewAll", locale)');
    expect(source).toContain('tCrm("crm.shell.sidebar.collapse", locale)');
    expect(source).toContain('tCrm("crm.shell.sidebar.expand", locale)');
    expect(source).toContain("rightContent: headerQuickLinks");
    expect(source).toContain("crm.shell.globalSearchAria");
    expect(source).toContain('href="/tasks"');
    expect(source).toContain('href="/activities"');
    expect(source).toContain('href="/support-inbox"');
    expect(source).toContain('href="/integrations"');
  });
});
