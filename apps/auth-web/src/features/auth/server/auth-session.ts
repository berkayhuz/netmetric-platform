import "server-only";

import { getAccessTokenFromCookieHeader } from "@netmetric/auth";
import { headers } from "next/headers";
import { cache } from "react";

import { authApi } from "../api/auth-api";
import type { AuthSessionStatus } from "../types/auth-session";

export const getCurrentAuthSession = cache(async (): Promise<AuthSessionStatus> => {
  const headerStore = await headers();
  if (!getAccessTokenFromCookieHeader(headerStore.get("cookie"))) {
    return {
      authenticated: false,
    };
  }

  try {
    return await authApi.getSessionStatus({ timeoutMs: 2_000 });
  } catch {
    return {
      authenticated: false,
    };
  }
});

export async function requireAuthenticatedSession(): Promise<AuthSessionStatus> {
  const session = await getCurrentAuthSession();

  if (!session.authenticated) {
    throw new Error("Authentication required.");
  }

  return session;
}
