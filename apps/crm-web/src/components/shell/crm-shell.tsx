"use client";

import * as React from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { ActivityIcon, CheckSquareIcon, InboxIcon, MegaphoneIcon, PlugIcon } from "lucide-react";
import {
  ClientRoutePerformanceReporter,
  CriticalRouteWarmup,
} from "@netmetric/observability/performance";
import {
  AppWorkspaceShell,
  compactActionControlsClassName,
  GlobalSearchDialog,
  isSafeGlobalSearchUrl,
  normalizeGlobalSearchSource,
  type AppHeaderNotifications,
  type AppSidebarNavGroup,
  type GlobalSearchResultItem,
  type GlobalSearchSourceGroup,
  createSharedWorkspaceUserMenuActions,
} from "@netmetric/ui/client";

import {
  canNavigateCrmModule,
  getCrmModuleById,
  crmModuleGroups,
  getCrmModulesByGroup,
  isCrmModuleNavigable,
} from "@/features/modules/module-registry";
import { crmNavIconColors, crmNavIcons } from "@/components/shell/crm-nav-icons";
import type { CrmCapabilities } from "@/lib/crm-auth/crm-capabilities";
import type { CrmShellNotification, CrmShellUser } from "@/lib/crm-auth/crm-session";
import { getCrmGroupLabel, getCrmModuleTitle, getCrmStatusLabel, tCrm } from "@/lib/i18n/crm-i18n";

import { crmCriticalRoutes } from "./crm-critical-routes";

const enableClientPerformanceReporting =
  process.env.NODE_ENV !== "production" || process.env.NEXT_PUBLIC_NETMETRIC_PERF === "1";

const crmSearchSourceOrder = [
  "crm",
  "account",
  "tools",
  "public",
  "other",
] as const satisfies readonly GlobalSearchSourceGroup[];
const crmSearchDefaultSources = ["crm", "tools", "public"] as const;

function normalizeSourcePath(
  url: string,
  source: ReturnType<typeof normalizeGlobalSearchSource>,
): string {
  const legacyPrefix = `/${source}/`;
  if (url === `/${source}`) {
    return "/";
  }

  if (url.startsWith(legacyPrefix)) {
    const normalized = url.slice(source.length + 1);
    return normalized.startsWith("/") ? normalized : `/${normalized}`;
  }

  return url;
}

function buildCrmNavGroups(
  locale: string,
  capabilities: CrmCapabilities | undefined,
): readonly AppSidebarNavGroup[] {
  return crmModuleGroups.reduce<AppSidebarNavGroup[]>((groups, group) => {
    const items = getCrmModulesByGroup(group)
      .filter(
        (moduleItem) =>
          !isCrmModuleNavigable(moduleItem) || canNavigateCrmModule(moduleItem, capabilities),
      )
      .map((moduleItem) => {
        const isNavigable = canNavigateCrmModule(moduleItem, capabilities);
        const label = getCrmModuleTitle(moduleItem, locale);
        const statusLabel = getCrmStatusLabel(moduleItem.status, locale);
        return {
          id: moduleItem.id,
          ...(isNavigable ? { href: moduleItem.path } : {}),
          label,
          icon: crmNavIcons[moduleItem.iconKey],
          iconClassName: crmNavIconColors[moduleItem.iconKey],
          title: statusLabel,
          ariaLabel: `${label} (${statusLabel})`,
          disabled: !isNavigable,
          match: moduleItem.path === "/dashboard" ? ("exact" as const) : ("prefix" as const),
        };
      });

    if (!items.length) {
      return groups;
    }

    groups.push({
      id: group,
      label: getCrmGroupLabel(group, locale),
      collapsible: true,
      defaultExpanded: group === "core",
      items,
    });

    return groups;
  }, []);
}

function getCrmModuleLabelById(moduleId: string, locale: string): string {
  const moduleItem = getCrmModuleById(moduleId);
  if (!moduleItem) {
    return moduleId;
  }

  return getCrmModuleTitle(moduleItem, locale);
}

export function CrmShell({
  children,
  locale,
  capabilities,
  user,
  notifications,
  authUrl,
  accountUrl,
  toolsUrl,
  publicUrl,
  backgroundStyle,
}: Readonly<{
  children: React.ReactNode;
  locale: string;
  capabilities?: CrmCapabilities;
  user: CrmShellUser;
  notifications: {
    items: CrmShellNotification[];
    unreadCount: number;
    unavailable: boolean;
  };
  authUrl: string;
  accountUrl: string;
  toolsUrl: string;
  publicUrl: string;
  backgroundStyle: React.CSSProperties;
}>) {
  // Temporary app wrapper: keeps CRM-specific capabilities/session/search/notification wiring
  // while AppWorkspaceShell owns shared shell/header/sidebar presentation.
  const pathname = usePathname() ?? "/";
  const router = useRouter();
  // Dev-only plain anchors prevent Turbopack from re-prefetching every visible CRM route on HMR.
  const shouldUsePlainNavigationLinks = process.env.NODE_ENV !== "production";
  const warmupPrefetch = React.useCallback((href: string) => router.prefetch(href), [router]);
  const navGroups = buildCrmNavGroups(locale, capabilities);
  const [isSearchOpen, setIsSearchOpen] = React.useState(false);
  const openSearch = React.useCallback(() => setIsSearchOpen(true), []);
  const navigateToSearchResult = React.useCallback(
    (url: string, result: GlobalSearchResultItem) => {
      if (isSafeGlobalSearchUrl(url)) {
        const source = normalizeGlobalSearchSource(result.source);
        const normalizedUrl = normalizeSourcePath(url, source);

        if (source === "crm" || source === "other") {
          router.push(normalizedUrl);
          return;
        }

        const targetBaseUrl =
          source === "account" ? accountUrl : source === "tools" ? toolsUrl : publicUrl;
        window.location.assign(new URL(normalizedUrl, targetBaseUrl).toString());
      }
    },
    [accountUrl, publicUrl, router, toolsUrl],
  );
  const unavailableActionTitle = tCrm("crm.shell.userMenu.unavailableAction", locale);
  const logout = () => {
    void (async () => {
      try {
        const response = await fetch("/api/auth/logout", {
          method: "POST",
          credentials: "same-origin",
          headers: {
            accept: "application/json",
          },
        });
        const payload = (await response.json().catch(() => null)) as {
          redirectUrl?: string;
        } | null;

        window.location.assign(payload?.redirectUrl ?? new URL("/login", authUrl).toString());
      } catch {
        window.location.assign(new URL("/login", authUrl).toString());
      }
    })();
  };
  const headerNotifications: AppHeaderNotifications = {
    items: notifications.items,
    unreadCount: notifications.unreadCount,
    label: tCrm("crm.shell.notifications.label", locale),
    triggerAriaLabel: tCrm("crm.shell.notifications.triggerAria", locale),
    unreadLabel: tCrm("crm.shell.notifications.unreadCount", locale, {
      count: notifications.unreadCount,
    }),
    emptyText: tCrm("crm.shell.notifications.empty", locale),
    error: notifications.unavailable ? tCrm("crm.shell.notifications.unavailable", locale) : null,
    viewAllLabel: tCrm("crm.shell.notifications.viewAll", locale),
    viewAllHref: "/notifications",
  };
  const { actions: userMenuActions, overflowActions: userMenuOverflowActions } =
    createSharedWorkspaceUserMenuActions({
      labels: {
        inviteUser: tCrm("crm.shell.userMenu.inviteUser", locale),
        settings: tCrm("crm.shell.userMenu.settings", locale),
        support: tCrm("crm.shell.userMenu.support", locale),
        createWorkspace: tCrm("crm.shell.userMenu.createWorkspace", locale),
        signOut: tCrm("crm.shell.userMenu.signOut", locale),
        unavailableAction: unavailableActionTitle,
      },
      onSignOut: logout,
      inviteUserHref: "http://localhost:7004/settings/team",
      settingsHref: "http://localhost:7004/profile",
      createWorkspaceHref: "http://localhost:7004/workspaces",
    });
  const headerQuickLinks = (
    <div className={`hidden items-center gap-1 md:flex ${compactActionControlsClassName}`}>
      {(() => {
        const tasksLabel = getCrmModuleLabelById("tasks", locale);
        const activitiesLabel = getCrmModuleLabelById("activities", locale);
        const supportInboxLabel = getCrmModuleLabelById("support-inbox", locale);
        const integrationsLabel = getCrmModuleLabelById("integration-hub", locale);

        return (
          <>
            <Link
              href="/tasks"
              prefetch={false}
              aria-label={tasksLabel}
              title={tasksLabel}
              className="top-chip inline-flex h-8 w-8 items-center justify-center rounded-sm border border-border bg-card text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
              <CheckSquareIcon aria-hidden="true" className="h-3.5 w-3.5" />
            </Link>
            <Link
              href="/activities"
              prefetch={false}
              aria-label={activitiesLabel}
              title={activitiesLabel}
              className="top-chip inline-flex h-8 w-8 items-center justify-center rounded-sm border border-border bg-card text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
              <ActivityIcon aria-hidden="true" className="h-3.5 w-3.5" />
            </Link>
            <Link
              href="/support-inbox"
              prefetch={false}
              aria-label={supportInboxLabel}
              title={supportInboxLabel}
              className="top-chip inline-flex h-8 w-8 items-center justify-center rounded-sm border border-border bg-card text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
              <InboxIcon aria-hidden="true" className="h-3.5 w-3.5" />
            </Link>
            <Link
              href="/integrations"
              prefetch={false}
              aria-label={integrationsLabel}
              title={integrationsLabel}
              className="top-chip inline-flex h-8 w-8 items-center justify-center rounded-sm border border-border bg-card text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
            >
              <PlugIcon aria-hidden="true" className="h-3.5 w-3.5" />
            </Link>
          </>
        );
      })()}
    </div>
  );
  const sidebarFooter = (
    <section
      aria-label="Workspace footer slot"
      className="rounded-md border border-dashed border-border/70 bg-muted/20 px-3 py-3"
    >
      <div className="flex items-start gap-2.5">
        <span className="mt-0.5 grid size-7 shrink-0 place-items-center rounded-sm bg-primary/10 text-primary">
          <MegaphoneIcon aria-hidden="true" className="size-3.5" />
        </span>
        <div className="min-w-0 space-y-1">
          <p className="text-sm font-medium text-foreground">Footer Slot</p>
          <p className="text-xs leading-5 text-muted-foreground">
            Future-ready area for upgrades, promotions, or workspace announcements.
          </p>
        </div>
      </div>
    </section>
  );

  return (
    <>
      <GlobalSearchDialog
        open={isSearchOpen}
        onOpenChange={setIsSearchOpen}
        searchEndpoint="/api/search"
        placeholder={tCrm("common.globalSearch.placeholder", locale)}
        defaultSources={crmSearchDefaultSources}
        sourceOrder={crmSearchSourceOrder}
        locale={locale}
        onNavigate={navigateToSearchResult}
        labels={{
          title: tCrm("common.globalSearch.title", locale),
          description: tCrm("common.globalSearch.description", locale),
          placeholder: tCrm("common.globalSearch.placeholder", locale),
          idle: tCrm("common.globalSearch.idle", locale),
          loading: tCrm("common.globalSearch.loading", locale),
          empty: tCrm("common.globalSearch.empty", locale),
          error: tCrm("common.globalSearch.error", locale),
          navigate: tCrm("common.globalSearch.guide.navigate", locale),
          open: tCrm("common.globalSearch.guide.open", locale),
          close: tCrm("common.globalSearch.guide.close", locale),
          escBadge: tCrm("common.globalSearch.badge.esc", locale),
          noResultsFor: tCrm("common.globalSearch.noResultsFor", locale),
        }}
      />
      <AppWorkspaceShell
        labels={{
          skipToContent: tCrm("crm.shell.skipToContent", locale),
          mobileSidebarAriaLabel: tCrm("crm.shell.appTitle", locale),
          mobileSidebarTitle: tCrm("crm.shell.appTitle", locale),
          appName: tCrm("crm.shell.appTitle", locale),
          workspace: tCrm("crm.shell.workspace", locale),
        }}
        pathname={pathname}
        navGroups={navGroups}
        sidebarNavStateKey="crm-web.sidebar.nav-groups"
        renderLink={({ href, className, children, ...linkProps }) =>
          shouldUsePlainNavigationLinks ? (
            <a href={href} className={className} {...linkProps}>
              {children}
            </a>
          ) : (
            <Link href={href} className={className} {...linkProps}>
              {children}
            </Link>
          )
        }
        user={user}
        userLabels={{
          triggerAriaLabel: tCrm("crm.shell.userMenu.triggerAria", locale, {
            name: user.displayName || user.email || tCrm("crm.shell.appTitle", locale),
          }),
          overflowAriaLabel: tCrm("crm.shell.userMenu.overflowAria", locale),
          fallbackName: user.email ?? tCrm("crm.shell.userFallback", locale),
          secondaryText: user.workspaceName ?? tCrm("crm.shell.workspace", locale),
        }}
        userActions={userMenuActions}
        userOverflowActions={userMenuOverflowActions}
        sidebarFooter={sidebarFooter}
        header={{
          mobileMenuAriaLabel: tCrm("crm.shell.appTitle", locale),
          collapseSidebarAriaLabel: tCrm("crm.shell.sidebar.collapse", locale),
          expandSidebarAriaLabel: tCrm("crm.shell.sidebar.expand", locale),
          className: "sticky top-0 z-40 bg-background/95",
          searchPlaceholder: tCrm("crm.shell.searchPlaceholder", locale),
          searchAriaLabel: tCrm("crm.shell.globalSearchAria", locale),
          searchReadOnly: true,
          onSearchClick: openSearch,
          onSearchFocus: openSearch,
          rightContent: headerQuickLinks,
          notifications: headerNotifications,
        }}
        backgroundStyle={backgroundStyle}
        contentFrameClassName="flex h-full min-h-0 w-full min-w-0 flex-col overflow-hidden rounded-md border border-border/70 bg-background/90 shadow-[0_24px_80px_rgb(15_23_42_/_0.12)] backdrop-blur space-y-4"
      >
        <ClientRoutePerformanceReporter
          app="crm-web"
          route={pathname}
          enabled={enableClientPerformanceReporting}
        />
        <CriticalRouteWarmup
          app="crm-web"
          routes={crmCriticalRoutes}
          currentPath={pathname}
          prefetch={warmupPrefetch}
          enabled={process.env.NODE_ENV === "production"}
        />
        {children}
      </AppWorkspaceShell>
    </>
  );
}
