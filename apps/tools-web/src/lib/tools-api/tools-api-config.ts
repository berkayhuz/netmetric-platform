import "server-only";

import { toolsEnv } from "@/lib/tools-env";

const defaultTimeoutMs = Number(process.env.TOOLS_API_TIMEOUT_MS ?? "15000");

export function getToolsApiConfig() {
  return {
    baseUrl: toolsEnv.toolsApiBaseUrl,
    defaultTimeoutMs: Number.isFinite(defaultTimeoutMs) ? defaultTimeoutMs : 15000,
  } as const;
}

export function joinToolsApiPath(route: string): string {
  const baseUrl = getToolsApiConfig().baseUrl;
  const normalized = route.startsWith("/") ? route : `/${route}`;
  return `${baseUrl}${normalized}`;
}
