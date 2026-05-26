import "server-only";

import { cache } from "react";
import { headers } from "next/headers";
import { resolveCurrentSession, type NetMetricSessionState } from "@netmetric/auth";

import { accountApiClient, AccountApiError, type AccountOverviewResponse } from "@/lib/account-api";
import { getAccountApiConfig } from "@/lib/account-api/account-api-config";

import {
  getAccountAccessCookieNames,
  getAccountApiAuthContext,
  getRequestCorrelationId,
} from "./account-auth-headers";

const publicAccountPathPrefixes = [
  "/service-unavailable",
  "/access-denied",
  "/retry-later",
  "/auth/continue",
] as const;

export function isPublicAccountPath(pathname: string): boolean {
  return publicAccountPathPrefixes.some(
    (prefix) => pathname === prefix || pathname.startsWith(`${prefix}/`),
  );
}

export type AccountSessionState =
  | {
      authenticated: true;
      overview: AccountOverviewResponse;
      auth: Extract<NetMetricSessionState, { isAuthenticated: true }>;
    }
  | {
      authenticated: false;
      auth: Extract<NetMetricSessionState, { isAuthenticated: false }>;
    };

export const getCurrentAccountAuthSession = cache(async (): Promise<NetMetricSessionState> => {
  const headerStore = await headers();

  return resolveCurrentSession({
    authBaseUrl: getAccountApiConfig().baseUrl,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getAccountAccessCookieNames(),
    correlationId: await getRequestCorrelationId(),
  });
});

export const getCurrentAccountSession = cache(async (): Promise<AccountSessionState> => {
  const authContext = await getAccountApiAuthContext();

  if (!authContext?.bearerToken) {
    return { authenticated: false, auth: { isAuthenticated: false } };
  }

  try {
    const requestOptions: {
      authContext: NonNullable<typeof authContext>;
      correlationId?: string;
    } = {
      authContext,
    };
    const correlationId = await getRequestCorrelationId();
    if (correlationId) {
      requestOptions.correlationId = correlationId;
    }

    const [auth, overview] = await Promise.all([
      getCurrentAccountAuthSession(),
      accountApiClient.getOverview(requestOptions),
    ]);

    if (!auth.isAuthenticated) {
      return { authenticated: false, auth };
    }

    return {
      authenticated: true,
      overview,
      auth,
    };
  } catch (error) {
    if (error instanceof AccountApiError) {
      if (error.kind === "unauthorized") {
        return { authenticated: false, auth: { isAuthenticated: false, status: 401 } };
      }
      if (error.kind === "server_error" || error.kind === "upstream_unavailable") {
        return { authenticated: false, auth: { isAuthenticated: false, status: 503 } };
      }
      if (error.kind === "rate_limited") {
        return { authenticated: false, auth: { isAuthenticated: false, status: 429 } };
      }
      if (error.kind === "forbidden") {
        return { authenticated: false, auth: { isAuthenticated: false, status: 403 } };
      }
    }

    throw error;
  }
});
