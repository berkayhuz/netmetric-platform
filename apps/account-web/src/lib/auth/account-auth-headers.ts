import "server-only";

import {
  DEFAULT_ACCESS_COOKIE_NAMES,
  getAccessTokenFromCookieHeader,
  getCorrelationIdFromHeaders,
} from "@netmetric/auth";
import { headers } from "next/headers";
import { cache } from "react";

import type { AccountApiAuthContext } from "@/lib/account-api";

const accessCookieEnv = process.env.ACCOUNT_ACCESS_COOKIE_NAME?.trim();
export function getAccountAccessCookieNames(): readonly string[] {
  return [
    ...new Set([...(accessCookieEnv ? [accessCookieEnv] : []), ...DEFAULT_ACCESS_COOKIE_NAMES]),
  ];
}

export const getAccountApiAuthContext = cache(
  async (): Promise<AccountApiAuthContext | undefined> => {
    const headerStore = await headers();
    const accessToken = getAccessTokenFromCookieHeader(
      headerStore.get("cookie"),
      getAccountAccessCookieNames(),
    );

    if (!accessToken) {
      return undefined;
    }

    return {
      bearerToken: accessToken,
    };
  },
);

export const getRequestCorrelationId = cache(async (): Promise<string | undefined> => {
  const headerStore = await headers();
  return getCorrelationIdFromHeaders(headerStore) ?? undefined;
});
