import "server-only";

import { cache } from "react";
import { headers } from "next/headers";
import { redirect } from "next/navigation";
import { fetchAuthJson, resolveCurrentSession, type NetMetricSessionState } from "@netmetric/auth";

import { buildAuthLoginRedirectUrl, buildCrmSessionResetUrl } from "@/lib/crm-auth/safe-return-url";
import { crmEnv, crmServerEnv } from "@/lib/crm-env";

import type { CrmCapabilities } from "./crm-capabilities";
import {
  createCrmCapabilities,
  crmCapabilityAllows,
  getRequiredCrmCapabilityForPath,
} from "./crm-capabilities";
import {
  getCrmAccessCookieNames,
  getCrmAccessToken,
  getRequestCorrelationId,
} from "./crm-auth-headers";

export type CrmSessionProfile = {
  tenantId: string;
  userId: string;
  sessionId: string;
  email: string;
  roles: string[];
  permissions: string[];
  accountStatus: string;
  emailConfirmed: boolean;
  mfaVerifiedAt: string | null;
};

export type CrmShellUser = {
  displayName: string;
  email: string | null;
  avatarUrl: string | null;
  workspaceName: string | null;
  sessionStatus: "authenticated";
};

export type CrmShellNotification = {
  id: string;
  title: string;
  description: string | null;
  occurredAt: string | null;
  isRead: boolean;
};

export type CrmSession = {
  accessToken: string;
  capabilities: CrmCapabilities;
  profile: CrmSessionProfile;
  faviconUrl: string | null;
  shellUser: CrmShellUser;
  shellNotifications: {
    items: CrmShellNotification[];
    unreadCount: number;
    unavailable: boolean;
  };
};

const publicCrmPathPrefixes = [
  "/access-denied",
  "/service-unavailable",
  "/retry-later",
  "/health",
] as const;

export function isPublicCrmPath(pathname: string): boolean {
  return publicCrmPathPrefixes.some(
    (prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`),
  );
}

export const getCurrentCrmAuthSession = cache(async (): Promise<NetMetricSessionState> => {
  const headerStore = await headers();

  return resolveCurrentSession({
    authBaseUrl: crmServerEnv.authSessionBaseUrl,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getCrmAccessCookieNames(),
    correlationId: await getRequestCorrelationId(),
  });
});

const getCachedValidatedSession = cache(async (pathname: string): Promise<CrmSession> => {
  const accessToken = await getCrmAccessToken();
  if (!accessToken) {
    redirect(buildAuthLoginRedirectUrl(pathname));
  }

  const session = await getCurrentCrmAuthSession();
  if (!session.isAuthenticated) {
    if (session.unavailable) {
      redirect("/service-unavailable");
    }

    if (session.status === 401) {
      redirect(buildCrmSessionResetUrl(pathname));
    }

    if (session.status === 403) {
      redirect("/access-denied");
    }

    if (session.status === 429) {
      redirect("/retry-later");
    }

    redirect(buildAuthLoginRedirectUrl(pathname));
  }

  const profile: CrmSessionProfile = {
    tenantId: session.user.tenantId,
    userId: session.user.userId,
    sessionId: session.user.sessionId,
    email: session.user.email,
    roles: [...session.user.roles],
    permissions: [...session.user.permissions],
    accountStatus: session.user.accountStatus,
    emailConfirmed: session.user.emailConfirmed,
    mfaVerifiedAt: session.user.mfaVerifiedAt,
  };

  if (!isCrmProfileAllowed(profile)) {
    redirect("/access-denied");
  }

  const capabilities = createCrmCapabilities(profile.permissions);
  const requiredCapability = getRequiredCrmCapabilityForPath(pathname);
  if (!crmCapabilityAllows(capabilities, requiredCapability)) {
    redirect("/access-denied");
  }

  const [accountOverview, shellNotifications, faviconUrl] = await Promise.all([
    getCrmShellAccountOverview(accessToken).catch(() => null),
    getCrmShellNotifications(accessToken).catch((): CrmSession["shellNotifications"] => ({
      items: [],
      unreadCount: 0,
      unavailable: true,
    })),
    getCrmFaviconUrl(accessToken).catch(() => null),
  ]);
  const [accountProfile, workspaceNameFromAuth] = await Promise.all([
    accountOverview?.displayName || accountOverview?.avatarUrl
      ? Promise.resolve<CrmShellAccountProfile | null>(null)
      : getCrmShellAccountProfile(accessToken).catch(() => null),
    accountOverview?.workspaceName
      ? Promise.resolve<string | null>(null)
      : getCrmWorkspaceNameFromAuth(accessToken, profile.tenantId).catch(() => null),
  ]);
  const shellUser = getCrmShellUser(
    profile.email,
    session.user.displayName,
    session.user.avatarUrl,
    accountOverview,
    accountProfile,
    workspaceNameFromAuth,
  );

  return {
    accessToken,
    capabilities,
    profile,
    faviconUrl,
    shellUser,
    shellNotifications,
  };
});

export async function validateCrmSession(pathname = "/"): Promise<CrmSession> {
  return getCachedValidatedSession(pathname);
}

const getCachedOptionalSession = cache(async (): Promise<CrmSession | null> => {
  const accessToken = await getCrmAccessToken();
  if (!accessToken) {
    return null;
  }

  const session = await getCurrentCrmAuthSession();
  if (!session.isAuthenticated) {
    return null;
  }

  const profile: CrmSessionProfile = {
    tenantId: session.user.tenantId,
    userId: session.user.userId,
    sessionId: session.user.sessionId,
    email: session.user.email,
    roles: [...session.user.roles],
    permissions: [...session.user.permissions],
    accountStatus: session.user.accountStatus,
    emailConfirmed: session.user.emailConfirmed,
    mfaVerifiedAt: session.user.mfaVerifiedAt,
  };

  if (!isCrmProfileAllowed(profile)) {
    return null;
  }

  const [accountOverview, shellNotifications, faviconUrl] = await Promise.all([
    getCrmShellAccountOverview(accessToken).catch(() => null),
    getCrmShellNotifications(accessToken).catch((): CrmSession["shellNotifications"] => ({
      items: [],
      unreadCount: 0,
      unavailable: true,
    })),
    getCrmFaviconUrl(accessToken).catch(() => null),
  ]);
  const [accountProfile, workspaceNameFromAuth] = await Promise.all([
    accountOverview?.displayName || accountOverview?.avatarUrl
      ? Promise.resolve<CrmShellAccountProfile | null>(null)
      : getCrmShellAccountProfile(accessToken).catch(() => null),
    accountOverview?.workspaceName
      ? Promise.resolve<string | null>(null)
      : getCrmWorkspaceNameFromAuth(accessToken, profile.tenantId).catch(() => null),
  ]);

  return {
    accessToken,
    capabilities: createCrmCapabilities(profile.permissions),
    profile,
    faviconUrl,
    shellUser: getCrmShellUser(
      profile.email,
      session.user.displayName,
      session.user.avatarUrl,
      accountOverview,
      accountProfile,
      workspaceNameFromAuth,
    ),
    shellNotifications,
  };
});

export async function getOptionalCrmShellSession(): Promise<CrmSession | null> {
  return getCachedOptionalSession();
}

function isCrmProfileAllowed(profile: CrmSessionProfile): boolean {
  return (
    profile.tenantId.length > 0 &&
    profile.userId.length > 0 &&
    profile.sessionId.length > 0 &&
    profile.accountStatus.toLowerCase() === "active" &&
    profile.emailConfirmed
  );
}

type AccountNotificationPayload = {
  id?: unknown;
  title?: unknown;
  description?: unknown;
  occurredAt?: unknown;
  isRead?: unknown;
};

type AccountNotificationsPayload = {
  items?: unknown;
  unreadCount?: unknown;
};

type AccountPreferencesPayload = {
  faviconUrl?: unknown;
};

type AccountProfilePayload = {
  displayName?: unknown;
  avatarUrl?: unknown;
};

type AccountOverviewOrganizationPayload = {
  organizationName?: unknown;
  isDefault?: unknown;
};

type AccountOverviewPayload = {
  displayName?: unknown;
  avatarUrl?: unknown;
  organizations?: unknown;
};

type CrmShellAccountOverview = {
  displayName: string | null;
  avatarUrl: string | null;
  workspaceName: string | null;
};

type CrmShellAccountProfile = {
  displayName: string | null;
  avatarUrl: string | null;
};

type AuthWorkspaceSummaryPayload = {
  tenantId?: unknown;
  organizationName?: unknown;
  isDefault?: unknown;
};

async function fetchAccountJson<TPayload>(
  accessToken: string,
  path: string,
): Promise<TPayload | null> {
  const headerStore = await headers();
  const correlationId = await getRequestCorrelationId();
  return fetchAuthJson<TPayload>({
    authBaseUrl: crmEnv.accountApiBaseUrl,
    path,
    accessToken,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getCrmAccessCookieNames(),
    correlationId,
  });
}

function toTrimmedStringOrNull(value: unknown): string | null {
  if (typeof value !== "string") {
    return null;
  }

  const trimmed = value.trim();
  return trimmed.length > 0 ? trimmed : null;
}

function resolveWorkspaceNameFromOverview(payload: AccountOverviewPayload | null): string | null {
  if (!payload || !Array.isArray(payload.organizations)) {
    return null;
  }

  const organizations = payload.organizations as AccountOverviewOrganizationPayload[];
  const activeWorkspace =
    organizations.find((organization) => organization?.isDefault === true) ?? organizations[0];

  return toTrimmedStringOrNull(activeWorkspace?.organizationName);
}

async function getCrmShellAccountOverview(
  accessToken: string,
): Promise<CrmShellAccountOverview | null> {
  const payload = await fetchAccountJson<AccountOverviewPayload>(
    accessToken,
    "/api/v1/account/overview",
  );
  if (!payload) {
    return null;
  }

  return {
    displayName: toTrimmedStringOrNull(payload.displayName),
    avatarUrl: toTrimmedStringOrNull(payload.avatarUrl),
    workspaceName: resolveWorkspaceNameFromOverview(payload),
  };
}

async function getCrmShellAccountProfile(
  accessToken: string,
): Promise<CrmShellAccountProfile | null> {
  const payload = await fetchAccountJson<AccountProfilePayload>(
    accessToken,
    "/api/v1/account/profile",
  );
  if (!payload) {
    return null;
  }

  return {
    displayName: toTrimmedStringOrNull(payload.displayName),
    avatarUrl: toTrimmedStringOrNull(payload.avatarUrl),
  };
}

async function getCrmWorkspaceNameFromAuth(
  accessToken: string,
  tenantId: string,
): Promise<string | null> {
  const headerStore = await headers();
  const correlationId = await getRequestCorrelationId();
  const payload = await fetchAuthJson<AuthWorkspaceSummaryPayload[]>({
    authBaseUrl: crmServerEnv.authSessionBaseUrl,
    path: "/api/auth/workspaces",
    accessToken,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getCrmAccessCookieNames(),
    correlationId,
  });

  if (!Array.isArray(payload)) {
    return null;
  }

  const normalizedTenantId = tenantId.trim().toLowerCase();
  const activeWorkspace =
    payload.find((workspace) => {
      const workspaceTenantId = toTrimmedStringOrNull(workspace?.tenantId);
      return workspaceTenantId?.toLowerCase() === normalizedTenantId;
    }) ??
    payload.find((workspace) => workspace?.isDefault === true) ??
    payload[0];

  return toTrimmedStringOrNull(activeWorkspace?.organizationName);
}

function getCrmShellUser(
  email: string,
  sessionDisplayName: string,
  sessionAvatarUrl: string | null,
  accountOverview: CrmShellAccountOverview | null,
  accountProfile: CrmShellAccountProfile | null,
  workspaceNameFromAuth: string | null,
): CrmShellUser {
  const fallbackDisplayName = sessionDisplayName.trim() || email;

  return {
    displayName: accountOverview?.displayName ?? accountProfile?.displayName ?? fallbackDisplayName,
    email,
    avatarUrl: accountOverview?.avatarUrl ?? accountProfile?.avatarUrl ?? sessionAvatarUrl,
    workspaceName: accountOverview?.workspaceName ?? workspaceNameFromAuth,
    sessionStatus: "authenticated",
  };
}

async function getCrmShellNotifications(
  accessToken: string,
): Promise<CrmSession["shellNotifications"]> {
  const payload = await fetchAccountJson<AccountNotificationsPayload>(
    accessToken,
    "/api/v1/account/notifications",
  );

  if (!payload || !Array.isArray(payload.items)) {
    return {
      items: [],
      unreadCount: 0,
      unavailable: true,
    };
  }

  return {
    items: payload.items.slice(0, 5).flatMap((item): CrmShellNotification[] => {
      const candidate = item as AccountNotificationPayload;
      if (typeof candidate.id !== "string" || typeof candidate.title !== "string") {
        return [];
      }

      return [
        {
          id: candidate.id,
          title: candidate.title,
          description: typeof candidate.description === "string" ? candidate.description : null,
          occurredAt: typeof candidate.occurredAt === "string" ? candidate.occurredAt : null,
          isRead: typeof candidate.isRead === "boolean" ? candidate.isRead : false,
        },
      ];
    }),
    unreadCount: typeof payload.unreadCount === "number" ? payload.unreadCount : 0,
    unavailable: false,
  };
}

async function getCrmFaviconUrl(accessToken: string): Promise<string | null> {
  const payload = await fetchAccountJson<AccountPreferencesPayload>(
    accessToken,
    "/api/v1/account/preferences",
  );
  return typeof payload?.faviconUrl === "string" && payload.faviconUrl.trim()
    ? payload.faviconUrl.trim()
    : null;
}
