import "server-only";

import { crmServerEnv } from "@/lib/crm-env";

const DEFAULT_TIMEOUT_MS = 15000;

type CrmApiConfig = {
  baseUrl: string;
  defaultTimeoutMs: number;
};

function normalizeBaseUrl(url: string): string {
  return url.replace(/\/+$/, "");
}

let cachedConfig: CrmApiConfig | undefined;

export function getCrmApiConfig(): CrmApiConfig {
  if (cachedConfig) {
    return cachedConfig;
  }

  cachedConfig = {
    baseUrl: normalizeBaseUrl(crmServerEnv.crmApiBaseUrl),
    defaultTimeoutMs: DEFAULT_TIMEOUT_MS,
  };

  return cachedConfig;
}

export function joinCrmApiPath(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${getCrmApiConfig().baseUrl}${normalizedPath}`;
}
