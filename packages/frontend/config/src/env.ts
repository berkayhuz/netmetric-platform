import { z } from "zod";

export const environmentStageSchema = z.enum(["development", "test", "staging", "production"]);

const baseFrontendEnvSchema = z.object({
  NODE_ENV: z.enum(["development", "test", "production"]).default("development"),
  APP_ENV: environmentStageSchema.default("development"),
  NEXT_PUBLIC_APP_NAME: z.string().min(1),
  NEXT_PUBLIC_API_BASE_URL: z.url(),
});

const optionalFrontendEnvSchema = z.object({
  SENTRY_DSN: z.url().optional(),
  NEXT_PUBLIC_SENTRY_DSN: z.url().optional(),
});

export const frontendEnvSchema = baseFrontendEnvSchema.extend(optionalFrontendEnvSchema.shape);

export type FrontendEnv = z.infer<typeof frontendEnvSchema>;

export const parseFrontendEnv = (input: Record<string, string | undefined>): FrontendEnv =>
  frontendEnvSchema.parse(input);
