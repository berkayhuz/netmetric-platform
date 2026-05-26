import "server-only";

import { z } from "zod";

const accountApiServerEnvSchema = z.object({
  ACCOUNT_API_BASE_URL: z.url().optional(),
  NEXT_PUBLIC_API_BASE_URL: z.url().optional(),
});

type AccountApiServerEnv = z.infer<typeof accountApiServerEnvSchema>;

function removeTrailingSlash(value: string): string {
  return value.replace(/\/+$/, "");
}

function resolveAccountApiBaseUrl(env: AccountApiServerEnv): string {
  const rawValue = env.ACCOUNT_API_BASE_URL ?? env.NEXT_PUBLIC_API_BASE_URL;

  if (!rawValue) {
    throw new Error(
      "Missing account API base URL. Set ACCOUNT_API_BASE_URL (preferred) or NEXT_PUBLIC_API_BASE_URL.",
    );
  }

  return removeTrailingSlash(rawValue);
}

const parsedEnv = accountApiServerEnvSchema.parse({
  ACCOUNT_API_BASE_URL: process.env.ACCOUNT_API_BASE_URL,
  NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
});

export function getAccountApiConfig() {
  return {
    baseUrl: resolveAccountApiBaseUrl(parsedEnv),
    defaultTimeoutMs: 15_000,
  } as const;
}

export function joinAccountApiPath(path: string): string {
  const normalizedPath = path.startsWith("/") ? path : `/${path}`;
  return `${getAccountApiConfig().baseUrl}${normalizedPath}`;
}
