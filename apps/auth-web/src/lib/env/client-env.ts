import { z } from "zod";

const clientEnvSchema = z.object({
  NEXT_PUBLIC_APP_NAME: z.string().min(1).default("NetMetric Auth"),
  NEXT_PUBLIC_APP_ORIGIN: z.url(),
  NEXT_PUBLIC_ACCOUNT_URL: z.url().optional(),
  NEXT_PUBLIC_TOOLS_URL: z.url().optional(),
  NEXT_PUBLIC_CRM_URL: z.url().optional(),
  NEXT_PUBLIC_PUBLIC_URL: z.url().optional(),
  NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS: z.string().optional(),
  NEXT_PUBLIC_API_BASE_URL: z.url().optional(),
  NEXT_PUBLIC_API_GATEWAY_BASE_URL: z.url().optional(),
});

export type ClientEnv = z.infer<typeof clientEnvSchema>;

export const clientEnv: ClientEnv = clientEnvSchema.parse({
  NEXT_PUBLIC_APP_NAME: process.env.NEXT_PUBLIC_APP_NAME,
  NEXT_PUBLIC_APP_ORIGIN: process.env.NEXT_PUBLIC_APP_ORIGIN,
  NEXT_PUBLIC_ACCOUNT_URL: process.env.NEXT_PUBLIC_ACCOUNT_URL,
  NEXT_PUBLIC_TOOLS_URL: process.env.NEXT_PUBLIC_TOOLS_URL,
  NEXT_PUBLIC_CRM_URL: process.env.NEXT_PUBLIC_CRM_URL,
  NEXT_PUBLIC_PUBLIC_URL: process.env.NEXT_PUBLIC_PUBLIC_URL,
  NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS: process.env.NEXT_PUBLIC_AUTH_ALLOWED_RETURN_ORIGINS,
  NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
  NEXT_PUBLIC_API_GATEWAY_BASE_URL: process.env.NEXT_PUBLIC_API_GATEWAY_BASE_URL,
});

if (!clientEnv.NEXT_PUBLIC_API_GATEWAY_BASE_URL && !clientEnv.NEXT_PUBLIC_API_BASE_URL) {
  throw new Error(
    "Missing API gateway base URL. Set NEXT_PUBLIC_API_GATEWAY_BASE_URL (preferred) or NEXT_PUBLIC_API_BASE_URL.",
  );
}
