import "server-only";

import {
  DEFAULT_ACCESS_COOKIE_NAMES,
  getAccessTokenFromCookieHeader,
  getCorrelationIdFromHeaders,
} from "@netmetric/auth";
import { headers } from "next/headers";

import { crmServerEnv } from "@/lib/crm-env";
import type { CrmApiAuthContext } from "@/lib/crm-api";

export function getCrmAccessCookieNames(): readonly string[] {
  const configuredName = crmServerEnv.accessCookieName?.trim();
  return [
    ...new Set([...(configuredName ? [configuredName] : []), ...DEFAULT_ACCESS_COOKIE_NAMES]),
  ];
}

export async function getCrmAccessToken(): Promise<string | undefined> {
  const headerStore = await headers();
  return (
    getAccessTokenFromCookieHeader(headerStore.get("cookie"), getCrmAccessCookieNames()) ??
    undefined
  );
}

export async function getCrmApiAuthContext(): Promise<CrmApiAuthContext | undefined> {
  const accessToken = await getCrmAccessToken();

  if (!accessToken) {
    return undefined;
  }

  return {
    bearerToken: accessToken,
  };
}

export async function getRequestCorrelationId(): Promise<string | undefined> {
  const headerStore = await headers();
  return getCorrelationIdFromHeaders(headerStore) ?? undefined;
}
