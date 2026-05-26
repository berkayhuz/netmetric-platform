"use client";

import * as React from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { Sparkles } from "lucide-react";
import {
  ClientRoutePerformanceReporter,
  CriticalRouteWarmup,
} from "@netmetric/observability/performance";
import {
  AppWorkspaceShell,
  GlobalSearchDialog,
  isSafeGlobalSearchUrl,
  normalizeGlobalSearchSource,
  type AppHeaderNotifications,
  type AppSidebarNavItem,
  type GlobalSearchResultItem,
  type GlobalSearchSourceGroup,
  createSharedWorkspaceUserMenuActions,
} from "@netmetric/ui/client";

import { accountCriticalRoutes } from "@/features/account/config/account-critical-routes";
import { accountNavIconColors, accountNavIcons } from "@/features/account/config/account-nav-icons";
import { accountRoutes } from "@/features/account/config/account-routes";
import { appEnv } from "@/lib/app-env";
import { tAccountClient } from "@/lib/i18n/account-i18n";

type AccountShellClientProps = {
  children: React.ReactNode;
  displayName: string;
  email: string | null;
  avatarUrl: string | null;
  workspaceName: string | null;
  notifications: AppHeaderNotifications;
  notificationsEndpoint: string | null;
  backgroundStyle: React.CSSProperties;
  locale: string;
};

const accountNavItems = accountRoutes.map((route) => ({
  href: route.href,
  label: route.label,
  icon: accountNavIcons[route.href],
  iconClassName: accountNavIconColors[route.href],
  match: "exact" as const,
})) satisfies readonly AppSidebarNavItem[];

const enableClientPerformanceReporting =
  process.env.NODE_ENV !== "production" || process.env.NEXT_PUBLIC_NETMETRIC_PERF === "1";

const accountSearchSourceOrder = [
  "account",
  "tools",
  "public",
  "crm",
  "other",
] as const satisfies readonly GlobalSearchSourceGroup[];
const accountSearchDefaultSources = ["account", "tools", "public"] as const;

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

export function AccountShellClient({
  children,
  displayName,
  email,
  avatarUrl,
  workspaceName,
  notifications,
  notificationsEndpoint,
  backgroundStyle,
  locale,
}: Readonly<AccountShellClientProps>) {
  const pathname = usePathname() ?? "/";
  const router = useRouter();
  const warmupPrefetch = React.useCallback((href: string) => router.prefetch(href), [router]);
  const [resolvedNotifications, setResolvedNotifications] =
    React.useState<AppHeaderNotifications>(notifications);
  const [isSearchOpen, setIsSearchOpen] = React.useState(false);
  const openSearch = React.useCallback(() => setIsSearchOpen(true), []);
  const navigateToSearchResult = React.useCallback(
    (url: string, result: GlobalSearchResultItem) => {
      if (isSafeGlobalSearchUrl(url)) {
        const source = normalizeGlobalSearchSource(result.source);
        const normalizedUrl = normalizeSourcePath(url, source);

        if (source === "account" || source === "other") {
          router.push(normalizedUrl);
          return;
        }

        const targetBaseUrl =
          source === "crm"
            ? appEnv.crmUrl
            : source === "tools"
              ? appEnv.toolsUrl
              : appEnv.publicUrl;
        window.location.assign(new URL(normalizedUrl, targetBaseUrl).toString());
      }
    },
    [router],
  );

  const notificationsEndpointRef = React.useRef(notificationsEndpoint);
  React.useEffect(() => {
    notificationsEndpointRef.current = notificationsEndpoint;
  }, [notificationsEndpoint]);

  React.useEffect(() => {
    const endpoint = notificationsEndpointRef.current;
    if (!endpoint) {
      return;
    }

    const controller = new AbortController();
    void (async () => {
      try {
        const response = await fetch(endpoint, {
          method: "GET",
          credentials: "same-origin",
          headers: {
            accept: "application/json",
          },
          signal: controller.signal,
        });

        if (!response.ok) {
          throw new Error("Notifications unavailable.");
        }

        const payload = (await response.json()) as Pick<
          AppHeaderNotifications,
          "items" | "unreadCount"
        >;
        const unreadCount = payload.unreadCount ?? 0;
        setResolvedNotifications((prev) => ({
          ...prev,
          items: payload.items ?? [],
          unreadCount,
          unreadLabel: `${unreadCount} unread`,
          loading: false,
          error: null,
        }));
      } catch {
        if (controller.signal.aborted) {
          return;
        }

        setResolvedNotifications((prev) => ({
          ...prev,
          loading: false,
          error: "Notifications are unavailable.",
        }));
      }
    })();

    return () => controller.abort();
  }, []);

  const unavailableActionTitle = tAccountClient("account.shell.userMenu.unavailableAction");
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
        const target = payload?.redirectUrl ?? new URL("/login", appEnv.authUrl).toString();
        window.location.assign(target);
      } catch {
        window.location.assign(new URL("/login", appEnv.authUrl).toString());
      }
    })();
  };
  const { actions: userMenuActions, overflowActions: userMenuOverflowActions } =
    createSharedWorkspaceUserMenuActions({
      labels: {
        inviteUser: tAccountClient("account.shell.userMenu.inviteUser"),
        settings: tAccountClient("account.shell.userMenu.settings"),
        support: tAccountClient("account.shell.userMenu.support"),
        createWorkspace: tAccountClient("account.shell.userMenu.createWorkspace"),
        signOut: tAccountClient("account.shell.userMenu.signOut"),
        unavailableAction: unavailableActionTitle,
      },
      onSignOut: logout,
      inviteUserHref: "/settings/team",
      settingsHref: "/preferences",
      createWorkspaceHref: "/workspaces",
    });
  const sidebarFooter = (
    <a
      href="https://tools.netmetric.net"
      target="_blank"
      rel="noreferrer"
      className="group block rounded-md border border-border/70 bg-muted/20 px-3 py-2.5 transition-colors hover:bg-muted/40 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
    >
      <div className="flex items-center gap-2">
        <span className="grid size-7 shrink-0 place-items-center rounded-sm bg-primary/10 text-primary">
          <Sparkles aria-hidden="true" className="size-3.5" />
        </span>
        <div className="min-w-0">
          <div className="truncate text-sm font-medium text-foreground">NetMetric Tools</div>
          <div className="truncate text-xs text-muted-foreground">Yeni araçlar yayında</div>
        </div>
      </div>
      <p className="mt-2 line-clamp-2 text-xs leading-5 text-muted-foreground">
        Free hesabınla QR, image converter ve yeni üretkenlik araçlarını dene.
      </p>
    </a>
  );

  return (
    <>
      <GlobalSearchDialog
        open={isSearchOpen}
        onOpenChange={setIsSearchOpen}
        searchEndpoint="/api/search"
        placeholder={tAccountClient("common.globalSearch.placeholder")}
        defaultSources={accountSearchDefaultSources}
        sourceOrder={accountSearchSourceOrder}
        locale={locale}
        onNavigate={navigateToSearchResult}
        labels={{
          title: tAccountClient("common.globalSearch.title"),
          description: tAccountClient("common.globalSearch.description"),
          placeholder: tAccountClient("common.globalSearch.placeholder"),
          idle: tAccountClient("common.globalSearch.idle"),
          loading: tAccountClient("common.globalSearch.loading"),
          empty: tAccountClient("common.globalSearch.empty"),
          error: tAccountClient("common.globalSearch.error"),
          navigate: tAccountClient("common.globalSearch.guide.navigate"),
          open: tAccountClient("common.globalSearch.guide.open"),
          close: tAccountClient("common.globalSearch.guide.close"),
          escBadge: tAccountClient("common.globalSearch.badge.esc"),
          noResultsFor: tAccountClient("common.globalSearch.noResultsFor"),
        }}
      />
      <AppWorkspaceShell
        labels={{
          skipToContent: tAccountClient("account.a11y.skipToContent"),
          mobileSidebarAriaLabel: tAccountClient("account.a11y.accountNavigation"),
          mobileSidebarTitle: tAccountClient("account.header.title"),
          appName: tAccountClient("account.header.title"),
        }}
        pathname={pathname}
        navGroups={[
          {
            id: "account",
            label: tAccountClient("account.header.title"),
            items: accountNavItems,
          },
        ]}
        renderLink={({ href, className, children, ...linkProps }) => (
          <Link href={href} className={className} {...{ prefetch: false }} {...linkProps}>
            {children}
          </Link>
        )}
        user={{
          displayName,
          email,
          avatarUrl,
        }}
        userLabels={{
          triggerAriaLabel: tAccountClient("account.shell.userMenu.triggerAria", {
            name: displayName,
          }),
          overflowAriaLabel: tAccountClient("account.shell.userMenu.overflowAria"),
          fallbackName: "Account user",
          secondaryText: workspaceName ?? tAccountClient("account.shell.workspace"),
        }}
        userActions={userMenuActions}
        userOverflowActions={userMenuOverflowActions}
        sidebarFooter={sidebarFooter}
        header={{
          mobileMenuAriaLabel: tAccountClient("account.a11y.accountNavigation"),
          collapseSidebarAriaLabel: tAccountClient("account.shell.sidebar.collapse"),
          expandSidebarAriaLabel: tAccountClient("account.shell.sidebar.expand"),
          className: "sticky top-0 z-40 bg-background/95",
          searchPlaceholder: "Search account, tools, public pages...",
          searchAriaLabel: "Search account workspace",
          searchReadOnly: true,
          onSearchClick: openSearch,
          onSearchFocus: openSearch,
          notifications: resolvedNotifications,
        }}
        backgroundStyle={backgroundStyle}
      >
        <ClientRoutePerformanceReporter
          app="account-web"
          route={pathname}
          enabled={enableClientPerformanceReporting}
        />
        <CriticalRouteWarmup
          app="account-web"
          routes={accountCriticalRoutes}
          currentPath={pathname}
          prefetch={warmupPrefetch}
          enabled={process.env.NODE_ENV === "production"}
        />
        {children}
      </AppWorkspaceShell>
    </>
  );
}
