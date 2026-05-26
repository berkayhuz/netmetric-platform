import { authRoutes } from "@/features/auth/config/auth-routes";

import { getSafePostLoginRedirectUrl } from "./post-login-url";

const defaultAuthenticatedUrl = getSafePostLoginRedirectUrl();

export function getRedirectAfterAuth(returnUrl?: string | null): string {
  const safeUrl = getSafePostLoginRedirectUrl(returnUrl);
  const safePathname = new URL(safeUrl).pathname;

  if (
    safePathname === authRoutes.login ||
    safePathname === authRoutes.register ||
    safePathname === authRoutes.forgotPassword
  ) {
    return defaultAuthenticatedUrl;
  }

  return safeUrl;
}
