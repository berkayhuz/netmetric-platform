import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createLoginSchema(v: ValidationText) {
  return z.object({
    // Backend accepts email or username via `emailOrUserName`.
    // Keep only required validation to avoid client-side false negatives.
    email: z.string().trim().min(1, v.emailRequired),
    password: z.string().min(1, v.passwordRequired),
    tenantId: z.string().trim().optional(),
    rememberMe: z.coerce.boolean().default(false),
    returnUrl: z.string().trim().optional(),
  });
}

export type LoginInput = z.infer<ReturnType<typeof createLoginSchema>>;
