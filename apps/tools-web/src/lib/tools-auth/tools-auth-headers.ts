import "server-only";

import { cache } from "react";
import {
  getAccessTokenFromCookieHeader,
  getCorrelationIdFromHeaders,
  resolveCurrentSession,
  type NetMetricSessionState,
} from "@netmetric/auth";
import { headers } from "next/headers";

import { toolsEnv } from "@/lib/tools-env";

const accessCookieEnv = process.env.TOOLS_ACCESS_COOKIE_NAME?.trim();
export function getToolsAccessCookieNames(): readonly string[] {
  return accessCookieEnv ? [accessCookieEnv] : [];
}

export async function getToolsAccessToken(): Promise<string | undefined> {
  const headerStore = await headers();
  return (
    getAccessTokenFromCookieHeader(headerStore.get("cookie"), getToolsAccessCookieNames()) ??
    undefined
  );
}

export async function getRequestCorrelationId(): Promise<string | undefined> {
  const headerStore = await headers();
  return getCorrelationIdFromHeaders(headerStore) ?? undefined;
}

export async function getToolsAuthStatus(): Promise<{ isAuthenticated: boolean }> {
  const session = await getCurrentToolsSession();
  return { isAuthenticated: session.isAuthenticated };
}

export const getCurrentToolsSession = cache(async (): Promise<NetMetricSessionState> => {
  const headerStore = await headers();

  return resolveCurrentSession({
    authBaseUrl: toolsEnv.apiBaseUrl,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getToolsAccessCookieNames(),
    correlationId: getCorrelationIdFromHeaders(headerStore),
  });
});
