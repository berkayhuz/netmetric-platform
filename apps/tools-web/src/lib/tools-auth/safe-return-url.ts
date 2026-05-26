import "server-only";

import { toolsEnv } from "@/lib/tools-env";

function toolsOrigin(): string {
  return new URL(toolsEnv.siteUrl).origin;
}

export function getSafeToolsReturnUrl(inputPath?: string): string {
  const fallback = `${toolsOrigin()}/`;

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

    const url = new URL(trimmed, toolsOrigin());
    return `${url.origin}${url.pathname}${url.search}${url.hash}`;
  }

  try {
    const absoluteUrl = new URL(trimmed);
    return absoluteUrl.origin === toolsOrigin()
      ? `${absoluteUrl.origin}${absoluteUrl.pathname}${absoluteUrl.search}${absoluteUrl.hash}`
      : fallback;
  } catch {
    return fallback;
  }
}

export function buildAuthLoginRedirectUrl(inputPath?: string): string {
  const loginUrl = new URL("/login", toolsEnv.authUrl);
  loginUrl.searchParams.set("returnUrl", getSafeToolsReturnUrl(inputPath));
  return loginUrl.toString();
}
