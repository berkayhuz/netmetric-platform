import "server-only";

import { z } from "zod";

const serverEnvSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  APP_ENV: z.enum(["development", "test", "staging", "production"]).default("development"),

  NEXT_PUBLIC_APP_NAME: z.string().min(1).default("NetMetric Auth"),
  NEXT_PUBLIC_APP_ORIGIN: z.url(),
  NEXT_PUBLIC_ACCOUNT_URL: z.url().optional(),
  NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS: z.string().optional(),
  NEXT_PUBLIC_API_BASE_URL: z.url().optional(),
  NEXT_PUBLIC_API_GATEWAY_BASE_URL: z.url().optional(),
});

export type ServerEnv = z.infer<typeof serverEnvSchema>;

export const serverEnv: ServerEnv = serverEnvSchema.parse({
  NODE_ENV: process.env.NODE_ENV,
  APP_ENV: process.env.APP_ENV,
  NEXT_PUBLIC_APP_NAME: process.env.NEXT_PUBLIC_APP_NAME,
  NEXT_PUBLIC_APP_ORIGIN: process.env.NEXT_PUBLIC_APP_ORIGIN,
  NEXT_PUBLIC_ACCOUNT_URL: process.env.NEXT_PUBLIC_ACCOUNT_URL,
  NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS: process.env.NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS,
  NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  NEXT_PUBLIC_API_GATEWAY_BASE_URL: process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL,
});

if (!serverEnv.NEXT_PUBLIC_API_GATEWAY_BASE_URL && !serverEnv.NEXT_PUBLIC_API_BASE_URL) {
  throw new Error(
    "Missing API gateway base URL. Set NEXT_PUBLIC_API_GATEWAY_BASE_URL (preferred) or NEXT_PUBLIC_API_BASE_URL.",
  );
}
