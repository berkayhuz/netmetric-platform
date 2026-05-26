import "server-only";

import { appEnv } from "@/lib/app-env";

function accountOrigin(): string {
  return new URL(appEnv.accountUrl).origin;
}

export function getSafeAccountReturnUrl(inputPath?: string): string {
  const fallback = `${accountOrigin()}/`;

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

    const url = new URL(trimmed, accountOrigin());
    return `${url.origin}${url.pathname}${url.search}${url.hash}`;
  }

  try {
    const absoluteUrl = new URL(trimmed);
    return absoluteUrl.origin === accountOrigin()
      ? `${absoluteUrl.origin}${absoluteUrl.pathname}${absoluteUrl.search}${absoluteUrl.hash}`
      : fallback;
  } catch {
    return fallback;
  }
}

export function buildAuthLoginRedirectUrl(inputPath?: string): string {
  const loginUrl = new URL("/login", appEnv.authUrl);
  loginUrl.searchParams.set("returnUrl", getSafeAccountReturnUrl(inputPath));
  return loginUrl.toString();
}

export function buildAccountSessionResetUrl(inputPath?: string): string {
  const resetUrl = new URL("/auth/session-reset", appEnv.accountUrl);
  resetUrl.searchParams.set("returnUrl", getSafeAccountReturnUrl(inputPath));
  return `${resetUrl.pathname}${resetUrl.search}`;
}
