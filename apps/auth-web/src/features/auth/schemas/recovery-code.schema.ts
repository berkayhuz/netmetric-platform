import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createRecoveryCodeSchema(v: ValidationText) {
  return z.object({
    identifier: z.string().trim().min(1, v.emailRequired),
    password: z.string().min(1, v.passwordRequired),
    recoveryCode: z.string().trim().min(6, v.recoveryCodeMin).max(64, v.recoveryCodeMax),
    challengeId: z.string().trim().optional(),
    returnUrl: z.string().trim().optional(),
  });
}

export type RecoveryCodeInput = z.infer<ReturnType<typeof createRecoveryCodeSchema>>;
