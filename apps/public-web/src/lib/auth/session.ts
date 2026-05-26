import "server-only";

import { cache } from "react";
import { headers } from "next/headers";
import {
  getCorrelationIdFromHeaders,
  resolveCurrentSession,
  type NetMetricSessionState,
} from "@netmetric/auth";

import { publicEnv } from "@/lib/public-env";

function getPublicAccessCookieNames(): readonly string[] {
  const accessCookieName = process.env.PUBLIC_ACCESS_COOKIE_NAME?.trim();
  return accessCookieName ? [accessCookieName] : [];
}

export const getCurrentPublicSession = cache(async (): Promise<NetMetricSessionState> => {
  const headerStore = await headers();

  return resolveCurrentSession({
    authBaseUrl: publicEnv.authSessionBaseUrl,
    cookieHeader: headerStore.get("cookie"),
    accessCookieNames: getPublicAccessCookieNames(),
    correlationId: getCorrelationIdFromHeaders(headerStore),
  });
});
