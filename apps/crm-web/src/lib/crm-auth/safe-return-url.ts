import "server-only";

import { crmPublicEnv } from "@/lib/crm-env";

function crmOrigin(): string {
  return new URL(crmPublicEnv.crmUrl).origin;
}

export function getSafeCrmReturnUrl(inputPath?: string): string {
  const fallback = `${crmOrigin()}/`;

  if (!inputPath) {
    return fallback;
  }

  const trimmed = inputPath.trim();
  if (!trimmed) {
    return fallback;
  }

  if (trimmed.startsWith("/")) {
    if (trimmed.startsWith("//")) {
      return fallback;
    }

    const url = new URL(trimmed, crmOrigin());
    return `${url.origin}${url.pathname}${url.search}${url.hash}`;
  }

  try {
    const absoluteUrl = new URL(trimmed);
    return absoluteUrl.origin === crmOrigin()
      ? `${absoluteUrl.origin}${absoluteUrl.pathname}${absoluteUrl.search}${absoluteUrl.hash}`
      : fallback;
  } catch {
    return fallback;
  }
}

export function buildAuthLoginRedirectUrl(inputPath?: string): string {
  const loginUrl = new URL("/login", crmPublicEnv.authUrl);
  loginUrl.searchParams.set("returnUrl", getSafeCrmReturnUrl(inputPath));
  return loginUrl.toString();
}

export function buildCrmSessionResetUrl(inputPath?: string): string {
  const resetUrl = new URL("/auth/session-reset", crmPublicEnv.crmUrl);
  resetUrl.searchParams.set("returnUrl", getSafeCrmReturnUrl(inputPath));
  return `${resetUrl.pathname}${resetUrl.search}`;
}
