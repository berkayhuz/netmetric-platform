import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createMfaSchema(v: ValidationText) {
  return z.object({
    identifier: z.string().trim().min(1, v.emailRequired),
    password: z.string().min(1, v.passwordRequired),
    code: z.string().trim().min(6, v.codeMin).max(12, v.codeMax),
    challengeId: z.string().trim().optional(),
    returnUrl: z.string().trim().optional(),
  });
}

export type MfaInput = z.infer<ReturnType<typeof createMfaSchema>>;
