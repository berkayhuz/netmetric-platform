import { createServerPerformanceLogger } from "@netmetric/observability/server";
import { getCurrentAccountSession, isPublicAccountPath } from "@/lib/auth/account-session";
import { loadAppBackgroundStyle } from "@netmetric/ui/app-background";
import { headers } from "next/headers";

import { AccountShellClient } from "./account-shell-client";

type AccountShellProps = {
  children: React.ReactNode;
  locale: string;
};

const accountShellPerformance = createServerPerformanceLogger({
  app: "account-web",
  component: "account-shell",
  enabled: process.env.NETMETRIC_PERF_LOG === "1" || process.env.NODE_ENV !== "production",
});

function resolveActiveWorkspaceName(
  session: Awaited<ReturnType<typeof getCurrentAccountSession>>,
): string | null {
  if (!session.authenticated) {
    return null;
  }

  const activeWorkspace =
    session.overview.organizations.find((organization) => organization.isDefault) ??
    session.overview.organizations[0];
  const workspaceName = activeWorkspace?.organizationName?.trim();

  return workspaceName ? workspaceName : null;
}

export async function AccountShell({ children, locale }: AccountShellProps) {
  // Temporary app wrapper: keeps account-specific session and notification wiring
  // while AppWorkspaceShell owns shared shell/header/sidebar presentation.
  const headerStore = await headers();
  const pathname = headerStore.get("x-netmetric-pathname") ?? "/";
  const isPublicPath = isPublicAccountPath(pathname);

  let session: Awaited<ReturnType<typeof getCurrentAccountSession>> | null = null;

  try {
    session = await accountShellPerformance.measure("shell.session", () =>
      getCurrentAccountSession(),
    );
  } catch (error) {
    if (isPublicPath) {
      session = { authenticated: false, auth: { isAuthenticated: false } };
    } else {
      throw error;
    }
  }

  const displayName = session?.authenticated ? session.overview.displayName : "Account user";
  const email = session?.authenticated ? session.auth.user.email : null;
  const avatarUrl = session?.authenticated ? (session.overview.avatarUrl ?? null) : null;
  const workspaceName = session?.authenticated ? resolveActiveWorkspaceName(session) : null;
  const backgroundStyle = loadAppBackgroundStyle();

  return (
    <AccountShellClient
      displayName={displayName}
      email={email}
      avatarUrl={avatarUrl}
      workspaceName={workspaceName}
      backgroundStyle={backgroundStyle}
      locale={locale}
      notificationsEndpoint={session?.authenticated ? "/api/account/shell-notifications" : null}
      notifications={{
        items: [],
        unreadCount: 0,
        loading: session?.authenticated ?? false,
        label: "Notifications",
        triggerAriaLabel: "Open notifications",
        emptyText: "No notifications yet.",
        viewAllLabel: "View all",
        viewAllHref: "/notifications",
        unreadLabel: "0 unread",
      }}
    >
      {children}
    </AccountShellClient>
  );
}
