import "server-only";

import { cookies } from "next/headers";
import { DEFAULT_ACCESS_COOKIE_NAMES } from "@netmetric/auth";

const cookieNames = {
  accessToken: "__Secure-netmetric-access",
  csrfToken: "__Secure-nm_csrf",
  activeTenantId: "nm_active_tenant",
} as const;

export type AuthCookieName = keyof typeof cookieNames;

export async function getAuthCookie(name: AuthCookieName): Promise<string | null> {
  const cookieStore = await cookies();

  if (name === "accessToken") {
    for (const candidate of DEFAULT_ACCESS_COOKIE_NAMES) {
      const value = cookieStore.get(candidate)?.value?.trim();
      if (value) {
        return value;
      }
    }
  }

  return cookieStore.get(cookieNames[name])?.value ?? null;
}

export async function getActiveTenantIdFromCookie(): Promise<string | null> {
  return getAuthCookie("activeTenantId");
}

export async function clearAuthWebCookies(): Promise<void> {
  const cookieStore = await cookies();

  cookieStore.delete(cookieNames.accessToken);
  for (const candidate of DEFAULT_ACCESS_COOKIE_NAMES) {
    cookieStore.delete(candidate);
  }
  cookieStore.delete(cookieNames.csrfToken);
  cookieStore.delete(cookieNames.activeTenantId);
}
