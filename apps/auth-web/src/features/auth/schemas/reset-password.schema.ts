import { z } from "zod";

import type { ValidationText } from "./validation-text";

export function createResetPasswordSchema(v: ValidationText) {
  return z
    .object({
      tenantId: z.string().trim().min(1, v.workspaceRequired),
      userId: z.string().trim().min(1, v.userIdRequired),
      token: z.string().trim().min(1, v.tokenRequired),
      password: z.string().min(8, v.passwordMin).max(128, v.passwordMax),
      confirmPassword: z.string().min(1, v.confirmPasswordRequired),
    })
    .refine((value) => value.password === value.confirmPassword, {
      path: ["confirmPassword"],
      message: v.passwordMatch,
    });
}

export type ResetPasswordInput = z.infer<ReturnType<typeof createResetPasswordSchema>>;
