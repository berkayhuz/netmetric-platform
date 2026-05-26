export const DEFAULT_AUTH_SESSION_ENDPOINT = "/api/auth/session-status";
export const DEFAULT_AUTH_LOGOUT_ENDPOINT = "/api/auth/logout";
export const DEFAULT_ACCOUNT_PROFILE_ENDPOINT = "/api/v1/account/overview";

export const DEFAULT_ACCESS_COOKIE_NAMES = [
  "netmetric-access",
  "__Secure-netmetric-access",
  "__Secure-nm_access",
] as const;

export const DEFAULT_REFRESH_COOKIE_NAME = "__Secure-netmetric-refresh";
export const DEFAULT_SESSION_COOKIE_NAME = "__Secure-netmetric-session";
export const DEFAULT_REFRESH_COOKIE_NAMES = [
  DEFAULT_REFRESH_COOKIE_NAME,
  "netmetric-refresh",
] as const;
export const DEFAULT_SESSION_COOKIE_NAMES = [
  DEFAULT_SESSION_COOKIE_NAME,
  "netmetric-session",
] as const;

export type NetMetricSessionUser = {
  tenantId: string;
  userId: string;
  sessionId: string;
  email: string;
  displayName: string;
  avatarUrl: string | null;
  roles: readonly string[];
  permissions: readonly string[];
  accountStatus: string;
  emailConfirmed: boolean;
  mfaVerifiedAt: string | null;
};

export type NetMetricSessionState =
  | {
      isAuthenticated: true;
      user: NetMetricSessionUser;
    }
  | {
      isAuthenticated: false;
      unavailable?: boolean;
      status?: number;
    };

export type NetMetricWorkspaceSummary = {
  tenantId: string;
  organizationName: string;
  organizationSlug: string | null;
  role: string | null;
  isDefault: boolean;
};

export type NetMetricCurrentUserProfile = {
  displayName: string | null;
  avatarUrl: string | null;
  activeWorkspace: NetMetricWorkspaceSummary | null;
  workspaces: readonly NetMetricWorkspaceSummary[];
};

export type ResolveCurrentSessionOptions = {
  authBaseUrl: string;
  cookieHeader?: string | null;
  accessCookieNames?: readonly string[] | undefined;
  sessionEndpoint?: string;
  correlationId?: string | null | undefined;
  fetchImpl?: typeof fetch;
};

export type LogoutFromAuthServiceOptions = AuthServiceHeaderOptions & {
  authBaseUrl: string;
  logoutEndpoint?: string;
  fetchImpl?: typeof fetch;
};

export type AuthServiceLogoutResult = {
  ok: boolean;
  status?: number;
  unavailable: boolean;
  setCookieHeaders: readonly string[];
};

export type AuthCookieDescriptor = {
  name: string;
  path: string;
};

export type AuthServiceHeaderOptions = {
  cookieHeader?: string | null | undefined;
  accessCookieNames?: readonly string[] | undefined;
  accessToken?: string | null | undefined;
  correlationId?: string | null | undefined;
  origin?: string | null | undefined;
  referer?: string | null | undefined;
  userAgent?: string | null | undefined;
  contentType?: string | null | undefined;
  accept?: string;
  includeCookieHeader?: boolean;
  includeBearerToken?: boolean;
};

export type FetchAuthJsonOptions = AuthServiceHeaderOptions & {
  authBaseUrl: string;
  path: string;
  fetchImpl?: typeof fetch;
};

export type FetchCurrentUserProfileOptions = AuthServiceHeaderOptions & {
  accountBaseUrl: string;
  profileEndpoint?: string;
  fetchImpl?: typeof fetch;
};

type SessionStatusPayload = {
  tenantId?: unknown;
  userId?: unknown;
  email?: unknown;
  sessionId?: unknown;
  displayName?: unknown;
  avatarUrl?: unknown;
  roles?: unknown;
  permissions?: unknown;
  accountStatus?: unknown;
  emailConfirmed?: unknown;
  mfaVerifiedAt?: unknown;
};

type AccountOverviewPayload = {
  displayName?: unknown;
  avatarUrl?: unknown;
  organizations?: unknown;
};

type AccountWorkspacePayload = {
  tenantId?: unknown;
  organizationName?: unknown;
  organizationSlug?: unknown;
  roles?: unknown;
  isDefault?: unknown;
};

export function normalizeAuthBaseUrl(value: string): string {
  return value.replace(/\/+$/, "");
}

export function joinAuthUrl(baseUrl: string, path: string): string {
  const safePath = path.startsWith("/") ? path : `/${path}`;
  return `${normalizeAuthBaseUrl(baseUrl)}${safePath}`;
}

export function uniqueAuthCookieNames(names: readonly string[] | undefined): readonly string[] {
  const configured = names?.map((name) => name.trim()).filter(Boolean) ?? [];
  return [...new Set([...configured, ...DEFAULT_ACCESS_COOKIE_NAMES])];
}

function uniqueCookieNames(
  configuredName: string | null | undefined,
  fallbackNames: readonly string[],
): readonly string[] {
  const configured = configuredName?.trim();
  return [...new Set([...(configured ? [configured] : []), ...fallbackNames])];
}

function decodeCookieValue(value: string): string {
  try {
    return decodeURIComponent(value);
  } catch {
    return value;
  }
}

export function getCookieValue(
  cookieHeader: string | null | undefined,
  name: string,
): string | null {
  if (!cookieHeader) {
    return null;
  }

  const prefix = `${name}=`;
  for (const part of cookieHeader.split(";")) {
    const trimmed = part.trim();
    if (trimmed.startsWith(prefix)) {
      return decodeCookieValue(trimmed.slice(prefix.length));
    }
  }

  return null;
}

export function getAccessTokenFromCookieHeader(
  cookieHeader: string | null | undefined,
  accessCookieNames?: readonly string[],
): string | null {
  for (const cookieName of uniqueAuthCookieNames(accessCookieNames)) {
    const token = getCookieValue(cookieHeader, cookieName)?.trim();
    if (token) {
      return token;
    }
  }

  return null;
}

export function createAuthCookieDescriptors({
  accessCookieName,
  refreshCookieName = DEFAULT_REFRESH_COOKIE_NAME,
  sessionCookieName = DEFAULT_SESSION_COOKIE_NAME,
}: {
  accessCookieName?: string | null | undefined;
  refreshCookieName?: string | null | undefined;
  sessionCookieName?: string | null | undefined;
} = {}): readonly AuthCookieDescriptor[] {
  const accessNames = uniqueAuthCookieNames(accessCookieName ? [accessCookieName] : undefined);
  const refreshNames = uniqueCookieNames(refreshCookieName, DEFAULT_REFRESH_COOKIE_NAMES);
  const sessionNames = uniqueCookieNames(sessionCookieName, DEFAULT_SESSION_COOKIE_NAMES);

  return [
    ...accessNames.map((name) => ({ name, path: "/" })),
    ...refreshNames.map((name) => ({ name, path: "/" })),
    ...sessionNames.map((name) => ({ name, path: "/" })),
    ...refreshNames.map((name) => ({ name, path: "/api/auth" })),
    ...sessionNames.map((name) => ({ name, path: "/api/auth" })),
  ];
}

export function shouldUseSecureAuthCookie(name: string, fallbackSecure: boolean): boolean {
  return name.startsWith("__Secure-") || name.startsWith("__Host-") || fallbackSecure;
}

export function getCorrelationIdFromHeaders(
  headers: Pick<Headers, "get"> | null | undefined,
): string | null {
  return headers?.get("x-request-id") ?? headers?.get("x-correlation-id") ?? null;
}

export function getSetCookieHeaders(headers: Headers): string[] {
  const withGetSetCookie = headers as Headers & {
    getSetCookie?: () => string[];
  };

  const cookies = withGetSetCookie.getSetCookie?.();
  if (cookies && cookies.length > 0) {
    return cookies;
  }

  const singleCookie = headers.get("set-cookie");
  return singleCookie ? [singleCookie] : [];
}

export function createAuthServiceHeaders({
  cookieHeader,
  accessCookieNames,
  accessToken,
  correlationId,
  origin,
  referer,
  userAgent,
  contentType,
  accept = "application/json",
  includeCookieHeader = true,
  includeBearerToken = true,
}: AuthServiceHeaderOptions = {}): Headers {
  const requestHeaders = new Headers({
    accept,
  });
  const resolvedAccessToken =
    accessToken?.trim() || getAccessTokenFromCookieHeader(cookieHeader, accessCookieNames);

  if (includeBearerToken && resolvedAccessToken) {
    requestHeaders.set("authorization", `Bearer ${resolvedAccessToken}`);
  }

  if (includeCookieHeader && cookieHeader) {
    requestHeaders.set("cookie", cookieHeader);
  }

  if (contentType) {
    requestHeaders.set("content-type", contentType);
  }

  if (origin) {
    requestHeaders.set("origin", origin);
  }

  if (referer) {
    requestHeaders.set("referer", referer);
  }

  if (userAgent) {
    requestHeaders.set("user-agent", userAgent);
  }

  if (correlationId) {
    requestHeaders.set("x-request-id", correlationId);
    requestHeaders.set("x-correlation-id", correlationId);
  }

  return requestHeaders;
}

export function createAuthLogoutRequestHeaders({
  contentType = "application/json",
  ...options
}: AuthServiceHeaderOptions = {}): Headers {
  return createAuthServiceHeaders({
    contentType,
    ...options,
  });
}

function stringArray(value: unknown): readonly string[] {
  return Array.isArray(value)
    ? value.filter((item): item is string => typeof item === "string")
    : [];
}

function readTrimmedString(value: unknown): string | null {
  return typeof value === "string" && value.trim() ? value.trim() : null;
}

export function mapAccountOverviewToCurrentUserProfile(
  payload: unknown,
): NetMetricCurrentUserProfile | null {
  if (!payload || typeof payload !== "object") {
    return null;
  }

  const candidate = payload as AccountOverviewPayload;
  const workspaces = Array.isArray(candidate.organizations)
    ? candidate.organizations.flatMap((item): NetMetricWorkspaceSummary[] => {
        if (!item || typeof item !== "object") {
          return [];
        }

        const workspace = item as AccountWorkspacePayload;
        const tenantId = readTrimmedString(workspace.tenantId);
        const organizationName = readTrimmedString(workspace.organizationName);

        if (!tenantId || !organizationName) {
          return [];
        }

        return [
          {
            tenantId,
            organizationName,
            organizationSlug: readTrimmedString(workspace.organizationSlug),
            role: stringArray(workspace.roles)[0] ?? null,
            isDefault: workspace.isDefault === true,
          },
        ];
      })
    : [];

  return {
    displayName: readTrimmedString(candidate.displayName),
    avatarUrl: readTrimmedString(candidate.avatarUrl),
    activeWorkspace: workspaces.find((workspace) => workspace.isDefault) ?? workspaces[0] ?? null,
    workspaces,
  };
}

export function mapSessionStatusToSafeSession(payload: unknown): NetMetricSessionState {
  if (!payload || typeof payload !== "object") {
    return { isAuthenticated: false };
  }

  const candidate = payload as SessionStatusPayload;
  if (
    typeof candidate.tenantId !== "string" ||
    typeof candidate.userId !== "string" ||
    typeof candidate.sessionId !== "string" ||
    typeof candidate.email !== "string" ||
    typeof candidate.accountStatus !== "string" ||
    typeof candidate.emailConfirmed !== "boolean"
  ) {
    return { isAuthenticated: false };
  }

  return {
    isAuthenticated: true,
    user: {
      tenantId: candidate.tenantId,
      userId: candidate.userId,
      sessionId: candidate.sessionId,
      email: candidate.email,
      displayName:
        typeof candidate.displayName === "string" && candidate.displayName.trim()
          ? candidate.displayName
          : candidate.email,
      avatarUrl:
        typeof candidate.avatarUrl === "string" && candidate.avatarUrl.trim()
          ? candidate.avatarUrl
          : null,
      roles: stringArray(candidate.roles),
      permissions: stringArray(candidate.permissions),
      accountStatus: candidate.accountStatus,
      emailConfirmed: candidate.emailConfirmed,
      mfaVerifiedAt:
        typeof candidate.mfaVerifiedAt === "string" || candidate.mfaVerifiedAt === null
          ? candidate.mfaVerifiedAt
          : null,
    },
  };
}

export async function resolveCurrentSession({
  authBaseUrl,
  cookieHeader,
  accessCookieNames,
  sessionEndpoint = DEFAULT_AUTH_SESSION_ENDPOINT,
  correlationId,
  fetchImpl = fetch,
}: ResolveCurrentSessionOptions): Promise<NetMetricSessionState> {
  const accessToken = getAccessTokenFromCookieHeader(cookieHeader, accessCookieNames);
  if (!accessToken) {
    return { isAuthenticated: false };
  }

  const requestHeaders = createAuthServiceHeaders({
    cookieHeader,
    accessCookieNames,
    accessToken,
    correlationId,
  });

  let response: Response;
  try {
    response = await fetchImpl(joinAuthUrl(authBaseUrl, sessionEndpoint), {
      method: "GET",
      headers: requestHeaders,
      cache: "no-store",
      redirect: "manual",
    });
  } catch {
    return { isAuthenticated: false, unavailable: true };
  }

  if (response.status === 401 || response.status === 403) {
    return { isAuthenticated: false, status: response.status };
  }

  if (!response.ok) {
    return { isAuthenticated: false, unavailable: true, status: response.status };
  }

  try {
    return mapSessionStatusToSafeSession(await response.json());
  } catch {
    return { isAuthenticated: false, unavailable: true, status: response.status };
  }
}

export async function logoutFromAuthService({
  authBaseUrl,
  logoutEndpoint = DEFAULT_AUTH_LOGOUT_ENDPOINT,
  fetchImpl = fetch,
  ...headerOptions
}: LogoutFromAuthServiceOptions): Promise<AuthServiceLogoutResult> {
  let response: Response;
  try {
    response = await fetchImpl(joinAuthUrl(authBaseUrl, logoutEndpoint), {
      method: "POST",
      headers: createAuthLogoutRequestHeaders(headerOptions),
      body: "{}",
      cache: "no-store",
      redirect: "manual",
    });
  } catch {
    return {
      ok: false,
      unavailable: true,
      setCookieHeaders: [],
    };
  }

  return {
    ok: response.ok,
    status: response.status,
    unavailable: false,
    setCookieHeaders: getSetCookieHeaders(response.headers),
  };
}

export async function fetchAuthJson<TPayload>({
  authBaseUrl,
  path,
  fetchImpl = fetch,
  ...headerOptions
}: FetchAuthJsonOptions): Promise<TPayload | null> {
  try {
    const response = await fetchImpl(joinAuthUrl(authBaseUrl, path), {
      method: "GET",
      headers: createAuthServiceHeaders(headerOptions),
      cache: "no-store",
      redirect: "manual",
    });

    if (!response.ok) {
      return null;
    }

    return (await response.json()) as TPayload;
  } catch {
    return null;
  }
}

export async function fetchCurrentUserProfile({
  accountBaseUrl,
  profileEndpoint = DEFAULT_ACCOUNT_PROFILE_ENDPOINT,
  fetchImpl = fetch,
  ...headerOptions
}: FetchCurrentUserProfileOptions): Promise<NetMetricCurrentUserProfile | null> {
  const payload = await fetchAuthJson<AccountOverviewPayload>({
    authBaseUrl: accountBaseUrl,
    path: profileEndpoint,
    fetchImpl,
    ...headerOptions,
  });

  return mapAccountOverviewToCurrentUserProfile(payload);
}
